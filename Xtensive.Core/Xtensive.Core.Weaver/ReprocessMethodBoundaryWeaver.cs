// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.25

using System;
using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Laos;
using PostSharp.Laos.Weaver;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Weaver.Resources;

namespace Xtensive.Core.Weaver
{
  internal class ReprocessMethodBoundaryWeaver : MethodLevelAspectWeaver, 
    IMethodLevelAdvice
  {
//    private static HashSet<object> processedMethods = new HashSet<object>();
    private IMethod onEntryMethod;
    private IMethod onErrorMethod;
    private IMethod onExitMethod;
    private IMethod onSuccessMethod;
    private MethodDefDeclaration targetMethodDef;
    private LocalVariableSymbol onEntryResult;
    private JoinPointKinds joinPointKinds;
    private InstructionSequence restartSequence;

    #region IMethodLevelAdvice Members

    public int Priority
    {
      get { return ((ILaosWeavableAspect) Aspect).AspectPriority; }
    }

    public MethodDefDeclaration Method
    {
      get { return targetMethodDef; }
    }

    public MetadataDeclaration Operand
    {
      get { return null; }
    }

    public JoinPointKinds JoinPointKinds {
      get { return joinPointKinds; }
    }

    public bool RequiresWeave(WeavingContext context)
    {
      return true;
    }

    public void Weave(WeavingContext context, InstructionBlock block)
    {
      var joinPointKind = context.JoinPoint.JoinPointKind;
      switch (joinPointKind) {
      case JoinPointKinds.BeforeMethodBody:
        WeaveOnEntry(context, block);
        break;
      case JoinPointKinds.AfterMethodBodyAlways:
        WeaveOnExit(context, block);
        break;
      case JoinPointKinds.AfterMethodBodySuccess:
        WeaveOnSuccess(context, block);
        break;
      case JoinPointKinds.AfterMethodBodyException:
        WeaveOnError(context, block);
        break;
      default:
        throw new InvalidOperationException(string.Format(
          Strings.ExUnexpectedJoinPointKindX, joinPointKind));
      }
    }

    #endregion

    protected override void OnTargetAssigned(bool reassigned)
    {
      targetMethodDef = (MethodDefDeclaration) TargetMethod;
    }

//    public override void ValidateInteractions(LaosAspectWeaver[] aspectsOnSameTarget)
//    {
//      // Uncomment this method to dump method-level aspect sequence
//
//      if (processedMethods.Contains(targetMethodDef))
//        return;
//      processedMethods.Add(targetMethodDef);
//
//      string target = string.Format("{0}.{1}", targetMethodDef.DeclaringType.Name, targetMethodDef.Name);
//      var sequence = new StringBuilder();
//      foreach (var a in aspectsOnSameTarget.Select(w => w.Aspect as ILaosMethodLevelAspect))
//        sequence.AppendFormat("{0} ({1})\n", a.GetType().Name, a.AspectPriority);
//      ErrorLog.Write(SeverityType.Warning,
//        "Sequence for {0} ({1}):\n{2}", target, joinPointKinds, sequence);
//    }

    public override void Implement()
    {
      base.Implement();
      if (targetMethodDef.MayHaveBody) {
        targetMethodDef.MethodBody.InitLocalVariables = true;
        Task.MethodLevelAdvices.Add(this);
      }
    }

    private void WeaveOnEntry(WeavingContext context, InstructionBlock block)
    {
      var module = block.Module;
      var methodBody = targetMethodDef.MethodBody;
      var objectType = module.FindType(typeof(object), BindingOptions.Default);

      onEntryResult = methodBody.RootInstructionBlock.DefineLocalVariable(objectType, "onEntryResult");

      var writer = context.InstructionWriter;
      var sequence = block.MethodBody.CreateInstructionSequence();
      block.AddInstructionSequence(sequence, NodePosition.Before, null);
      restartSequence = sequence; // Reprocessing point
      
      writer.AttachInstructionSequence(sequence);
      writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      writer.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstructionMethod(OpCodeNumber.Callvirt, onEntryMethod);
      writer.EmitInstructionLocalVariable(OpCodeNumber.Stloc, onEntryResult);
      writer.DetachInstructionSequence();
    }

    private void WeaveOnExit(WeavingContext context, InstructionBlock block)
    {
      var writer = context.InstructionWriter;
      var methodBody = block.MethodBody;
      var sequence = methodBody.CreateInstructionSequence();
      block.AddInstructionSequence(sequence, NodePosition.Before, null);

      writer.AttachInstructionSequence(sequence);
      writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      writer.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, onEntryResult);
      writer.EmitInstructionMethod(OpCodeNumber.Callvirt, onExitMethod);
      writer.DetachInstructionSequence();
    }

    private void WeaveOnSuccess(WeavingContext context, InstructionBlock block)
    {
      var writer = context.InstructionWriter;
      var methodBody = block.MethodBody;
      var sequence = methodBody.CreateInstructionSequence();
      block.AddInstructionSequence( sequence, NodePosition.Before, null );

      writer.AttachInstructionSequence( sequence );
      writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      writer.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, onEntryResult);
      writer.EmitInstructionMethod(OpCodeNumber.Callvirt, onSuccessMethod);
      writer.DetachInstructionSequence();
    }

    private void WeaveOnError(WeavingContext context, InstructionBlock block)
    {
      var writer = context.InstructionWriter;
      var module = block.Module;
      var methodBody = block.MethodBody;
      var exceptionLocal = methodBody.RootInstructionBlock.DefineLocalVariable(module.Cache.GetType(typeof(Exception)), "~exception~{0}");
      var sequence = methodBody.CreateInstructionSequence();
      block.AddInstructionSequence( sequence, NodePosition.Before, null );
      
      writer.AttachInstructionSequence( sequence );
      writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      writer.EmitInstructionLocalVariable(OpCodeNumber.Stloc, exceptionLocal);
      writer.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, exceptionLocal);
      writer.EmitInstructionMethod(OpCodeNumber.Callvirt, onErrorMethod);
      
      var rethrowSequence = methodBody.CreateInstructionSequence();
      block.AddInstructionSequence(rethrowSequence, NodePosition.After, null);
      var reprocessSequence = methodBody.CreateInstructionSequence();
      block.AddInstructionSequence(reprocessSequence, NodePosition.After, null);
      var skipSequence = methodBody.CreateInstructionSequence();
      block.AddInstructionSequence(skipSequence, NodePosition.After, null);
      
      writer.EmitSwitchInstruction(
        new[] {rethrowSequence, reprocessSequence, skipSequence});
      writer.DetachInstructionSequence();

      writer.AttachInstructionSequence(rethrowSequence);
      writer.EmitInstruction(OpCodeNumber.Rethrow);
      writer.DetachInstructionSequence();

      writer.AttachInstructionSequence(reprocessSequence);
      writer.EmitBranchingInstruction(OpCodeNumber.Leave, restartSequence);
      writer.DetachInstructionSequence();
    }

    public override void Initialize()
    {
      base.Initialize();
	  
      var module = Task.Project.Module;
      onEntryMethod = (IMethod) module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ReprocessMethodBoundaryAspect).GetMethod("OnEntry"), BindingOptions.RequireGenericDefinition));
      onExitMethod = (IMethod) module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ReprocessMethodBoundaryAspect).GetMethod("OnExit"), BindingOptions.RequireGenericDefinition));
      onSuccessMethod = (IMethod) module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ReprocessMethodBoundaryAspect).GetMethod("OnSuccess"), BindingOptions.RequireGenericDefinition));
      onErrorMethod = (IMethod) module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ReprocessMethodBoundaryAspect).GetMethod("OnError"), BindingOptions.RequireGenericDefinition));

      var aspectType = MethodLevelAspect.GetType();
      var baseType = typeof (ReprocessMethodBoundaryAspect);
      var onExit = aspectType.GetMethod("OnExit");
      var onSuccess = aspectType.GetMethod("OnSuccess");
      var onError = aspectType.GetMethod("OnError");

      joinPointKinds = JoinPointKinds.BeforeMethodBody;
      if (onExit.DeclaringType != baseType)
        joinPointKinds |= JoinPointKinds.AfterMethodBodyAlways;
      if (onSuccess.DeclaringType != baseType)
        joinPointKinds |= JoinPointKinds.AfterMethodBodySuccess;
      if (onError.DeclaringType != baseType)
        joinPointKinds |= JoinPointKinds.AfterMethodBodyException;
    }
  }
}
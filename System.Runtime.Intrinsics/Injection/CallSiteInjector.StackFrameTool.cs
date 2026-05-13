using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

using InlineIL;

namespace RiceTea.Backport.Injection;

unsafe partial class CallSiteInjector
{
    /*
     * for .NET Framework 4.6+, .NET Core and .NET 5+
     * we can use reflection + delegate pointers to optimize the performance to get specific stack frame.
     */
    private static class StackFrameTool
    {
        private delegate void StackFrameFieldsGetter(object instance, out IntPtr[] methodHandles, out int[] offsets);

        private static readonly Type? _type, _type2;
        private static readonly void* _initializeSourceInfoFunc_Long, _initializeSourceInfoFunc_Short,
            _typeConstructor_Long, _typeConstructor_Short, _type2Constructor, _runtimeHandleConstructor;
        private static readonly StackFrameFieldsGetter? _stackFrameHelperGetterDelegate;

        static StackFrameTool()
        {
            Type? type, type2;
            ConstructorInfo? runtimeHandleConstructor, constructor, constructor2, constructor3;
            MethodInfo? initializeSourceInfoMethod;
            StackFrameFieldsGetter stackFrameHelperGetterDelegate;
            bool isLongConstructor, isLongSourceInfoMethod;

            try
            {
                type = Type.GetType("System.Diagnostics.StackFrameHelper");
                type2 = Type.GetType("System.RuntimeMethodInfoStub");
                Type? type3 = Type.GetType("System.RuntimeMethodHandleInternal");
                Type? typeInterface = Type.GetType("System.IRuntimeMethodInfo");
                if (type is null || type2 is null || typeInterface is null)
                    return;

                runtimeHandleConstructor = typeof(RuntimeMethodHandle).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, [typeInterface], null);
                if (runtimeHandleConstructor is null)
                    return;

                constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Array.Empty<Type>(), null);
                if (constructor is null)
                {
                    constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, [typeof(Thread)], null);
                    if (constructor is null)
                        return;
                    isLongConstructor = true;
                }
                else
                {
                    isLongConstructor = false;
                }
                if (type3 is null)
                {
                    constructor2 = type2.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, [typeof(IntPtr), typeof(object)], null);
                    if (constructor2 is null)
                        return;
                    constructor3 = null;
                }
                else
                {
                    constructor2 = type2.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, [type3, typeof(object)], null);
                    constructor3 = type3.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(IntPtr)], null);
                    if (constructor2 is null || constructor3 is null)
                        return;
                }

                initializeSourceInfoMethod = type.GetMethod("InitializeSourceInfo", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(bool), typeof(Exception)], null);
                if (initializeSourceInfoMethod is null || initializeSourceInfoMethod.ReturnType != typeof(void))
                {
                    initializeSourceInfoMethod = type.GetMethod("InitializeSourceInfo", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(int), typeof(bool), typeof(Exception)], null);
                    if (initializeSourceInfoMethod is null || initializeSourceInfoMethod.ReturnType != typeof(void))
                        return;
                    isLongSourceInfoMethod = true;
                }
                else
                {
                    isLongSourceInfoMethod = false;
                }
                FieldInfo? rgMethodHandleField = type.GetField("rgMethodHandle", BindingFlags.NonPublic | BindingFlags.Instance);
                if (rgMethodHandleField is null || rgMethodHandleField.FieldType != typeof(IntPtr[]))
                    return;

                FieldInfo? rgiOffsetField = type.GetField("rgiOffset", BindingFlags.NonPublic | BindingFlags.Instance);
                if (rgiOffsetField is null || rgiOffsetField.FieldType != typeof(int[]))
                    return;

                DynamicMethod stackFrameHelperGetter = new DynamicMethod(
                    name: "StackFrameHelperGetter", 
                    returnType: typeof(void), 
                    parameterTypes: new Type[] { typeof(object), typeof(IntPtr[]).MakeByRefType(), typeof(int[]).MakeByRefType() }, 
                    owner: type, 
                    skipVisibility: true);

                ILGenerator generator = stackFrameHelperGetter.GetILGenerator();
                generator.DeclareLocal(type);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Castclass, type);
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldfld, rgMethodHandleField);
                generator.Emit(OpCodes.Stind_Ref);
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldfld, rgiOffsetField);
                generator.Emit(OpCodes.Stind_Ref);
                generator.Emit(OpCodes.Ret);

                stackFrameHelperGetterDelegate = (StackFrameFieldsGetter)stackFrameHelperGetter.CreateDelegate(typeof(StackFrameFieldsGetter));
                stackFrameHelperGetterDelegate.Method.MethodHandle.GetFunctionPointer();
            }
            catch (Exception)
            {
                return;
            }
            _type = type;
            _type2 = type2;
            _runtimeHandleConstructor = (void*)runtimeHandleConstructor.MethodHandle.GetFunctionPointer();
            if (isLongConstructor)
                _typeConstructor_Long = (void*)constructor.MethodHandle.GetFunctionPointer();
            else
                _typeConstructor_Short = (void*)constructor.MethodHandle.GetFunctionPointer();
            _type2Constructor = (void*)constructor2.MethodHandle.GetFunctionPointer();
            if (isLongSourceInfoMethod)
                _initializeSourceInfoFunc_Long = (void*)initializeSourceInfoMethod.MethodHandle.GetFunctionPointer();
            else
                _initializeSourceInfoFunc_Short = (void*)initializeSourceInfoMethod.MethodHandle.GetFunctionPointer();
            _stackFrameHelperGetterDelegate = stackFrameHelperGetterDelegate;
        }

        public static bool IsSupported => _type is not null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object CreateUninitializedObject(Type type)
#if NETSTANDARD2_0
                => System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
#else
                => RuntimeHelpers.GetUninitializedObject(type);
#endif


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool TryGetNativeFrame(int skipFrames, out RuntimeMethodHandle methodHandle, out int offset)
        {
            object? stackFrameHelper = null;
            try
            {
                void* constructor = _typeConstructor_Short;
                if (constructor is null)
                {
                    constructor = _typeConstructor_Long;
                    if (constructor is null)
                        goto Failed;
                    stackFrameHelper = CreateUninitializedObject(_type!);
                    IL.Push(stackFrameHelper);
                    IL.Emit.Ldnull();
                    IL.Push(constructor);
                    IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(void), [typeof(Thread)]));
                }
                else
                {
                    stackFrameHelper = CreateUninitializedObject(_type!);
                    IL.Push(stackFrameHelper);
                    IL.Push(constructor);
                    IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(void)));
                }

                void* initializeSourceInfoFunc = _initializeSourceInfoFunc_Short;
                if (initializeSourceInfoFunc is null)
                {
                    initializeSourceInfoFunc = _initializeSourceInfoFunc_Long;
                    if (initializeSourceInfoFunc is null)
                        goto Failed;
                    IL.Push(stackFrameHelper);
                    IL.Push(0);
                    IL.Push(false);
                    IL.Emit.Ldnull();
                    IL.Push(initializeSourceInfoFunc);
                    IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(void), [typeof(int), typeof(bool), typeof(Exception)]));
                }
                else
                {
                    IL.Push(stackFrameHelper);
                    IL.Push(false);
                    IL.Emit.Ldnull();
                    IL.Push(initializeSourceInfoFunc);
                    IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(void), [typeof(bool), typeof(Exception)]));
                }

                StackFrameFieldsGetter? getterDelegate = _stackFrameHelperGetterDelegate;
                if (getterDelegate is null)
                    goto Failed;
                getterDelegate.Invoke(stackFrameHelper, out IntPtr[] rgMethodHandle, out int[] rgiOffset);

                int iNumOfFrames = rgMethodHandle.Length;

                IL.Emit.Ldtoken(new MethodRef(typeof(StackFrameTool), nameof(TryGetNativeFrame)));
                IL.Pop(out RuntimeMethodHandle selfMethodHandle);
                skipFrames += CalculateExtraSkipFrames(stackFrameHelper, rgMethodHandle, selfMethodHandle) + 1; // self frame

                if (
                    (iNumOfFrames - skipFrames) <= 0 ||
                    // StackTrace.CalculateFramesToSkip(StackFrameHelper, int) is too slow and creates lots of temporary RuntimeMethodInfo, we use a faster way to get same output
                    !TryGetHandle(stackFrameHelper, rgMethodHandle[skipFrames], out methodHandle)
                    )
                    goto Failed;

                offset = rgiOffset[skipFrames];
            }
            catch (Exception)
            {
                goto Failed;
            }
            finally
            {
                (stackFrameHelper as IDisposable)?.Dispose();
            }

            return true;

        Failed:
            methodHandle = default;
            offset = default;
            return false;
        }

        private static bool TryGetHandle(object stackFrameHelper, IntPtr handle, out RuntimeMethodHandle methodHandle)
        {
            if (handle == IntPtr.Zero)
            {
                methodHandle = default;
                return false;
            }
            object stub = CreateUninitializedObject(_type2!);
            IL.Push(stub);
            IL.Push(handle);
            IL.Push(stackFrameHelper);
            IL.Push(_type2Constructor);
            // for .NET Framework and .NET Core, the signiture is .ctor(IntPtr, object)
            // for .NET 5+, the signiture is .ctor(RuntimeMethodHandleInternal, object)
            // but RuntimeMethodHandleInternal just a wrapper structure for IntPtr, we can ignore the wrapper, just passes raw IntPtr argument!
            IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(void), [typeof(IntPtr), typeof(object)]));

            IL.PushOutRef(out methodHandle);
            IL.Push(stub);
            IL.Push(_runtimeHandleConstructor);
            IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(void), [typeof(object)]));
            return true;
        }

        private static int CalculateExtraSkipFrames(object stackFrameHelper, IntPtr[] rgMethodHandle, RuntimeMethodHandle archorHandle)
        {
            void* archorAddress = null;
            int i = 0;
            foreach (IntPtr handle in rgMethodHandle)
            {
                if (TryGetHandle(stackFrameHelper, rgMethodHandle[i], out RuntimeMethodHandle methodHandle))
                {
                    if (archorHandle == methodHandle)
                        break;
                    if (archorAddress is null)
                        archorAddress = FindRealEntryPoint(archorHandle);
                    if (archorAddress == FindRealEntryPoint(methodHandle))
                        break;
                }
                i++;
            }
            return i;
        }
    }
}
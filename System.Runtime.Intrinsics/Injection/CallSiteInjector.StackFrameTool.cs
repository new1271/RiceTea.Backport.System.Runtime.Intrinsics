using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

using InlineIL;

namespace RiceTea.Backport.Injection;

unsafe partial class CallSiteInjector
{
    private static class StackFrameTool
    {
        private static readonly Type? _type, _type2, _type3;
        private static readonly FieldInfo? _rgMethodHandleField, _rgiOffsetField, _iFrameCountField;
        private static readonly delegate* managed<object, int, int> _calculateFramesToSkipFunc;
        private static readonly void* _initializeSourceInfoFunc_Long, _initializeSourceInfoFunc_Short,
            _typeConstructor_Long, _typeConstructor_Short, _type2Constructor, _type3Constructor, _runtimeHandleConstructor;

        static StackFrameTool()
        {
            Type? type = Type.GetType("System.Diagnostics.StackFrameHelper");
            Type? type2 = Type.GetType("System.RuntimeMethodInfoStub");
            Type? type3 = Type.GetType("System.RuntimeMethodHandleInternal");
            Type? typeInterface = Type.GetType("System.IRuntimeMethodInfo");
            if (type is null || type2 is null || typeInterface is null)
                return;

            ConstructorInfo? runtimeHandleConstructor = typeof(RuntimeMethodHandle).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, [typeInterface], null);
            if (runtimeHandleConstructor is null)
                return;

            bool isLongConstructor;
            ConstructorInfo? constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Array.Empty<Type>(), null);
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
            ConstructorInfo? constructor2, constructor3;
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

            bool isLongSourceInfoMethod;
            MethodInfo? initializeSourceInfoMethod = type.GetMethod("InitializeSourceInfo", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(bool), typeof(Exception)], null);
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

            MethodInfo? calculateFramesToSkipMethod = typeof(StackTrace).GetMethod("CalculateFramesToSkip", BindingFlags.Static | BindingFlags.NonPublic, null, [type, typeof(int)], null);
            if (calculateFramesToSkipMethod is null || calculateFramesToSkipMethod.ReturnType != typeof(int))
                return;

            FieldInfo? iFrameCountField = type.GetField("iFrameCount", BindingFlags.NonPublic | BindingFlags.Instance);
            if (iFrameCountField is null || iFrameCountField.FieldType != typeof(int))
                return;

            FieldInfo? rgMethodHandleField = type.GetField("rgMethodHandle", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rgMethodHandleField is null || rgMethodHandleField.FieldType != typeof(IntPtr[]))
                return;

            FieldInfo? rgiOffsetField = type.GetField("rgiOffset", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rgiOffsetField is null || rgiOffsetField.FieldType != typeof(int[]))
                return;

            _type = type;
            _type2 = type2;
            _type3 = type3;
            _runtimeHandleConstructor = (void*)runtimeHandleConstructor.MethodHandle.GetFunctionPointer();
            if (isLongConstructor)
                _typeConstructor_Long = (void*)constructor.MethodHandle.GetFunctionPointer();
            else
                _typeConstructor_Short = (void*)constructor.MethodHandle.GetFunctionPointer();
            _type2Constructor = (void*)constructor2.MethodHandle.GetFunctionPointer();
            _type3Constructor = constructor3 is null ? null : (void*)constructor3.MethodHandle.GetFunctionPointer();
            if (isLongSourceInfoMethod)
                _initializeSourceInfoFunc_Long = (void*)initializeSourceInfoMethod.MethodHandle.GetFunctionPointer();
            else
                _initializeSourceInfoFunc_Short = (void*)initializeSourceInfoMethod.MethodHandle.GetFunctionPointer();
            _calculateFramesToSkipFunc = (delegate*<object, int, int>)calculateFramesToSkipMethod.MethodHandle.GetFunctionPointer();
            _iFrameCountField = iFrameCountField;
            _rgMethodHandleField = rgMethodHandleField;
            _rgiOffsetField = rgiOffsetField;
        }

        public static bool IsSupported => _type is not null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object CreateUninitializedObject(Type type)
#if NETSTANDARD2_0
                => FormatterServices.GetUninitializedObject(type);
#else
                => RuntimeHelpers.GetUninitializedObject(type);
#endif


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool TryGetNativeFrame(int skipFrames, out RuntimeMethodHandle handle, out int offset)
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

                IntPtr[] rgHandles = ((IntPtr[])_rgMethodHandleField!.GetValue(stackFrameHelper)!);
                int iNumOfFrames = rgHandles.Length;

                IL.Emit.Ldtoken(new MethodRef(typeof(StackFrameTool), nameof(TryGetNativeFrame)));
                IL.Pop(out RuntimeMethodHandle selfMethodHandle);
                skipFrames += CalculateExtraSkipFrames(stackFrameHelper, rgHandles, selfMethodHandle) + 1; // self frame

                if ((iNumOfFrames - skipFrames) <= 0 || !TryGetHandle(stackFrameHelper, rgHandles[skipFrames], out handle))
                    goto Failed;

                offset = ((int[])_rgiOffsetField!.GetValue(stackFrameHelper)!)[skipFrames];
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
            handle = default;
            offset = default;
            return false;
        }

        private static bool TryGetHandle(object stackFrameHelper, IntPtr rgHandle, out RuntimeMethodHandle handle)
        {
            if (rgHandle == IntPtr.Zero)
            {
                handle = default;
                return false;
            }
            object stub = CreateUninitializedObject(_type2!);
            IL.Push(stub);
            IL.Push(rgHandle);
            IL.Push(stackFrameHelper);
            IL.Push(_type2Constructor);
            IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(void), [typeof(IntPtr), typeof(object)]));

            IL.PushOutRef(out handle);
            IL.Push(stub);
            IL.Push(_runtimeHandleConstructor);
            IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(void), [typeof(object)]));
            return true;
        }

        private static int CalculateExtraSkipFrames(object stackFrameHelper, IntPtr[] rgHandles, RuntimeMethodHandle archorHandle)
        {
            void* archorAddress = null;
            int i = 0;
            foreach (IntPtr rgHandle in rgHandles)
            {
                if (TryGetHandle(stackFrameHelper, rgHandles[i], out RuntimeMethodHandle handle))
                {
                    if (archorHandle == handle)
                        break;
                    if (archorAddress is null)
                        archorAddress = FindRealEntryPoint(archorHandle);
                    if (archorAddress == FindRealEntryPoint(handle))
                        break;
                }
                i++;
            }
            return i;
        }
    }
}
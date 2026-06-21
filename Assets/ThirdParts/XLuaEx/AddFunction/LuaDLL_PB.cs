
namespace XLua.LuaDLL
{

    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using XLua;
    public partial class Lua
    {
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb(System.IntPtr L);

        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadPB(System.IntPtr L)
        {
            return luaopen_pb(L);
        }


        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb_io(System.IntPtr L);

        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadPB_IO(System.IntPtr L)
        {
            return luaopen_pb_io(L);
        }


        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb_conv(System.IntPtr L);

        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadPB_CONV(System.IntPtr L)
        {
            return luaopen_pb_conv(L);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb_slice(System.IntPtr L);

        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadPB_SLICE(System.IntPtr L)
        {
            return luaopen_pb_slice(L);
        }

        

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb_buffer(System.IntPtr L);

        [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadPB_BUFFER(System.IntPtr L)
        {
            return luaopen_pb_buffer(L);
        }
    }
}

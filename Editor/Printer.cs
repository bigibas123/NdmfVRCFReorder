using UnityEngine;

namespace tk.dingemans.bigibas123.NdmfVrcfReorder.Extensions
{
    public static class Printer
    {
        public static void Print<T>(this T o, string arg)
        {
            int lastDot = o.GetType().Namespace.LastIndexOf(".") + 1;
            string name = o.GetType().Namespace.Substring(lastDot) + "." + o.GetType().Name;
            
            Debug.Log("["+name+"] " + arg);
        }
    }
}
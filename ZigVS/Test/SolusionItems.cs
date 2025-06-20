/********************************************************************************************
Copyright(c) 2023 LuckyStar Studio LLC
All rights reserved.

Microsoft Public License (Ms-PL)

This license governs use of the accompanying software. If you use the software, you
accept this license. If you do not accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
same meaning here as under U.S. copyright law.
A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions
and limitations in section 3, each contributor grants you a non-exclusive, worldwide,
royalty-free copyright license to reproduce its contribution, prepare derivative works of
its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions
and limitations in section 3, each contributor grants you a non-exclusive, worldwide,
royalty-free license under its licensed patents to make, have made, use, sell, offer for
sale, import, and/or otherwise dispose of its contribution in the software or derivative
works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors'
name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are
infringed by the software, your patent license from such contributor to the software ends
automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent,
trademark, and attribution notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only
under this license by including a complete copy of this license with your distribution.
If you distribute any portion of the software in compiled or object code form, you may only
do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it.The contributors give
no express warranties, guarantees, or conditions. You may have additional consumer rights
under your local laws which this license cannot change. To the extent permitted under your
local laws, the contributors exclude the implied warranties of merchantability, fitness for
a particular purpose and non-infringement.

********************************************************************************************/

namespace ZigVS.Test
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
#nullable enable
#pragma warning disable VSTHRD010

    static class SolusionItems
    {
        public static IEnumerable<IVsHierarchy?> GetProjects(this IVsSolution i_IVsSolution)
        {
            //          Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            IEnumHierarchies l_projectsIEnumHierarchies;
            var l_resultInt = i_IVsSolution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLINSOLUTION, Guid.Empty, out l_projectsIEnumHierarchies);

            if (l_resultInt == VSConstants.S_OK)
            {
                IVsHierarchy?[] l_IVsHierarchyArray = new IVsHierarchy?[1] { null };
                uint l_fetchedUint = 0;
                for (l_projectsIEnumHierarchies.Reset();
                    l_projectsIEnumHierarchies.Next(1, l_IVsHierarchyArray, out l_fetchedUint) == VSConstants.S_OK && l_fetchedUint == 1;
                    /*nothing*/)
                {
                    yield return l_IVsHierarchyArray[0];
                }
            }
        }

        public static IEnumerable<string> GetProjectItems(this IVsHierarchy i_IVsHierarchy)
        {
            return i_IVsHierarchy.GetProjectItems(VSConstants.VSITEMID_ROOT);
        }

        public static IEnumerable<string> GetProjectItems(this IVsHierarchy i_IVsHierarchy, uint i_itemIDuint)
        {
            //            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            object l_itemObject;

            // Get first l_itemObject
            i_IVsHierarchy.GetProperty(i_itemIDuint, (int)__VSHPROPID.VSHPROPID_FirstVisibleChild, out l_itemObject);

            while (l_itemObject != null)
            {
                string l_canonicalNameString;
                i_IVsHierarchy.GetCanonicalName((uint)(int)l_itemObject, out l_canonicalNameString);
                if (!string.IsNullOrWhiteSpace(l_canonicalNameString))
                    yield return l_canonicalNameString;

                // Call recursively for children
                foreach (var child in i_IVsHierarchy.GetProjectItems((uint)(int)l_itemObject))
                    yield return child;

                // Get next sibling
                i_IVsHierarchy.GetProperty((uint)(int)l_itemObject, (int)__VSHPROPID.VSHPROPID_NextVisibleSibling, out l_itemObject);
            }
        }
    }
#pragma warning restore VSTHRD010
}
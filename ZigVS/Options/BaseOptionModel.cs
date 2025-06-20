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

namespace ZigVS
{
    using Microsoft;
    using Microsoft.VisualStudio.Settings;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Shell.Settings;
    using Microsoft.VisualStudio.Threading;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading.Tasks;
    using Task = System.Threading.Tasks.Task;

    internal abstract class BaseOptionModel<T> where T : BaseOptionModel<T>, new()
    {
        private static AsyncLazy<T> m_liveModel = new AsyncLazy<T>(CreateAsync, ThreadHelper.JoinableTaskFactory);
        private static AsyncLazy<ShellSettingsManager> m_ShellSettingsManager =
            new AsyncLazy<ShellSettingsManager>(GetSettingsManagerAsync, ThreadHelper.JoinableTaskFactory);

        protected BaseOptionModel() { }

        public static T Instance
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

#pragma warning disable VSTHRD104 // Offer async methods
                return ThreadHelper.JoinableTaskFactory.Run(GetLiveInstanceAsync);
#pragma warning restore VSTHRD104 // Offer async methods
            }
        }

        public static Task<T> GetLiveInstanceAsync() => m_liveModel.GetValueAsync();

        public static async Task<T> CreateAsync()
        {
            var i_instanceT = new T();
            await i_instanceT.LoadAsync();
            return i_instanceT;
        }

        protected virtual string CollectionName { get; } = typeof(T).FullName;

        public virtual void Load()
        {
            ThreadHelper.JoinableTaskFactory.Run(LoadAsync);
        }

        public virtual async Task LoadAsync()
        {
            ShellSettingsManager l_ShellSettingsManager = await m_ShellSettingsManager.GetValueAsync();
            SettingsStore l_SettingsStore = l_ShellSettingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);

            if (!l_SettingsStore.CollectionExists(CollectionName))
            {
                return;
            }

            foreach (PropertyInfo l_PropertyInfo in GetOptionProperties())
            {
                try
                {
                    string l_serializedPropString = l_SettingsStore.GetString(CollectionName, l_PropertyInfo.Name);
                    object l_deserializedObject = DeserializeValue(l_serializedPropString, l_PropertyInfo.PropertyType);
                    l_PropertyInfo.SetValue(this, l_deserializedObject);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                }
            }
        }

        public virtual void Save()
        {
            ThreadHelper.JoinableTaskFactory.Run(SaveAsync);
        }

        public virtual async Task SaveAsync()
        {
            ShellSettingsManager l_ShellSettingsManager = await m_ShellSettingsManager.GetValueAsync();
            WritableSettingsStore l_WritableSettingsStore = l_ShellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            if (!l_WritableSettingsStore.CollectionExists(CollectionName))
            {
                l_WritableSettingsStore.CreateCollection(CollectionName);
            }

            foreach (PropertyInfo l_PropertyInfo in GetOptionProperties())
            {
                string l_serializedString = SerializeValue(l_PropertyInfo.GetValue(this));
                l_WritableSettingsStore.SetString(CollectionName, l_PropertyInfo.Name, l_serializedString);
            }

            T l_liveObject = await GetLiveInstanceAsync();

            if (this != l_liveObject)
            {
                await l_liveObject.LoadAsync();
            }
        }

        protected virtual string SerializeValue(object i_object)
        {
            using (var l_MemoryStream = new MemoryStream())
            {
                var l_BinaryFormatter = new BinaryFormatter();
                l_BinaryFormatter.Serialize(l_MemoryStream, i_object);
                l_MemoryStream.Flush();
                return Convert.ToBase64String(l_MemoryStream.ToArray());
            }
        }

        sealed class ForcedSerializationBinder : SerializationBinder
        {
            public override Type BindToType(string l_assemblyString, string l_typeNameString)
            {
                String exeAssembly = Assembly.GetExecutingAssembly().FullName;

                Type typeToDeserialize = Type.GetType(String.Format("{0}, {1}", l_typeNameString, exeAssembly));

                return typeToDeserialize;
            }
        }

        protected virtual object DeserializeValue(string i_serializedObject, Type i_objectType)
        {
            byte[] l_byteArray = Convert.FromBase64String(i_serializedObject);

            using (var l_MemoryStream = new MemoryStream(l_byteArray))
            {
                var l_BinaryFormatter = new BinaryFormatter();
                l_BinaryFormatter.Binder = new ForcedSerializationBinder();
                return l_BinaryFormatter.Deserialize(l_MemoryStream);
            }
        }

        private static async Task<ShellSettingsManager> GetSettingsManagerAsync()
        {
#pragma warning disable VSTHRD010 
            // False-positive in Threading Analyzers. Bug tracked here https://github.com/Microsoft/vs-threading/issues/230
            var l_IVsSettingsManager = await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SVsSettingsManager)) as IVsSettingsManager;
#pragma warning restore VSTHRD010 

            Assumes.Present(l_IVsSettingsManager);

            return new ShellSettingsManager(l_IVsSettingsManager);
        }

        private IEnumerable<PropertyInfo> GetOptionProperties()
        {
            return GetType()
                .GetProperties()
                .Where(p => p.PropertyType.IsSerializable && p.PropertyType.IsPublic);
        }
    }
}
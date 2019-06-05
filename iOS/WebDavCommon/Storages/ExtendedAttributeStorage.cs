using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using WebDavCommon.Helpers;

namespace WebDavCommon.Storages
{
    /// <summary>This class provides methods for read and write of <see cref="ExtendedAttribute"/></summary>
    public class ExtendedAttributeStorage
    {
        /// <summary>The extended attribute stream name. </summary>
        private const string ExtendedAttributeKey = "FsExtensionMetadata";

        /// <summary>The _attribute formatter.</summary>
        private readonly IFormatter _attributeFormatter;

        /// <summary>Initializes a new instance of the <see cref="ExtendedAttributeStorage"/> class.</summary>
        public ExtendedAttributeStorage()
        {
            this._attributeFormatter = new BinaryFormatter();
        }

        /// <summary>The get file extended attribute.</summary>
        /// <param name="path">The local path.</param>
        /// <returns>The <see cref="ExtendedAttribute"/>.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="path"/> is null. </exception>
        /// <exception cref="SerializationException"> if deserializing is failed. </exception>
        public ExtendedAttribute Get(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            byte[] extendedAttribute = NSFileManagerHelper.GetExtendedAttributeBytes(path, ExtendedAttributeKey);
            if (extendedAttribute == null)
            {
                return null;
            }
        
            using (MemoryStream stream = new MemoryStream(extendedAttribute))
            {
                return (ExtendedAttribute)this._attributeFormatter.Deserialize(stream);
            }
        }

        /// <summary>Write extended attribute.</summary>
        /// <param name="path">The path.</param>
        /// <param name="extendedAttribute">The file extended attribute.</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="path"/> is null. </exception>
        /// <exception cref="ArgumentNullException"> if <paramref name="extendedAttribute"/> is null. </exception>
        /// <exception cref="SerializationException"> if serializing is failed. </exception>
        public void Write(string path, ExtendedAttribute extendedAttribute)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (extendedAttribute == null) throw new ArgumentNullException(nameof(extendedAttribute));

            using (MemoryStream stream = new MemoryStream())
            {
                this._attributeFormatter.Serialize(stream, extendedAttribute);
                stream.Flush();
                stream.Position = 0;
                NSFileManagerHelper.SetExtendedAttributeBytes(path, ExtendedAttributeKey, stream.ToArray());
            }
        }

        /// <summary>The delete extended attribute.</summary>
        /// <param name="path">The local path.</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="path"/> is null. </exception>
        public void Delete(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            NSFileManagerHelper.DeleteExtendedAttribute(path, ExtendedAttributeStorage.ExtendedAttributeKey);
        }
    }
}
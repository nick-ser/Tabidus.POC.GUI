using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tabidus.POC.Common;
using Tabidus.POC.Common.Model.Endpoint;

namespace Tabidus.POC.GUI.Common
{
    /// <summary>
    /// Class Directory Computer Importer data.
    /// </summary>
    public class DirectoryComputerImporter
    {
        /// <summary>
        /// The splic e_ character
        /// </summary>
        private const char SPLICE_CHAR = '\\';
        /// <summary>
        /// The _file path
        /// </summary>
        private string _filePath;

        /// <summary>
        /// The _directory computer collections
        /// </summary>
        private List<DirectoryComputerItem> _directoryComputerCollections;
        /// <summary>
        /// Gets the directory computer collections.
        /// </summary>
        /// <value>The directory computer collections.</value>
        public List<DirectoryComputerItem> DirectoryComputerCollections
        {
            get
            {
                return _directoryComputerCollections;
            }
            set
            {
                _directoryComputerCollections = value;
            }
        }
        /// <summary>
        /// The _is valid
        /// </summary>
        private bool _isValid = true;
        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid
        {
            get
            {
                return _isValid;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryComputerImporter"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public DirectoryComputerImporter(string filePath, List<DirectoryComputerItem> list)
        {
            _filePath = filePath;
            LoadDataFile(list);
        }

        /// <summary>
        /// Builds the tree.
        /// </summary>
        /// <param name="parentId">The parent identifier.</param>
        /// <param name="parentName">Name of the parent.</param>
        /// <param name="line">The line.</param>
        /// <param name="level">The level.</param>
        public void BuildTree(int parentId,string rootName, string parentName, string[] line, int level)
        {
            if (line.Length > level)
            {
                string name = line[level].Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    _isValid = line.Length - 1 == level;
                    return; //Break
                }

                bool isComputer = line.Length - 1 == level && !string.IsNullOrWhiteSpace(line[level]);
                string parentPath = rootName;
                for (int i = 0; i < level; i++)
                {
                    parentPath += line[i];
                }
                var existItem = FindItem(name, parentPath, level, !isComputer);

                if (existItem == null)
                {
                    if (isComputer)
                    {
                        existItem = DirectoryComputerFactory.CreateComputer(name);
                    }
                    else
                    {
                        existItem = DirectoryComputerFactory.CreateDirectory(name);
                    }
                    existItem.ParentId = parentId;
                    existItem.ParentName = parentName;
                    existItem.ParentPath = parentPath;
                    existItem.Level = level;

                    _directoryComputerCollections.Add(existItem);
                }
                BuildTree(existItem.Id,rootName, name, line, level + 1);
            }
        }
        /// <summary>
        /// Finds the item.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parentName">Name of the parent.</param>
        /// <param name="level">The level.</param>
        /// <param name="isDirectory">if set to <c>true</c> [is directory].</param>
        /// <returns>DirectoryComputerItem.</returns>
        private DirectoryComputerItem FindItem(string name, string parentName, int level, bool isDirectory)
        {
            return _directoryComputerCollections.FirstOrDefault(c => c.Level == level && c.ParentPath == parentName && c.Name == name && c.IsDirectory == isDirectory);
        }
        /// <summary>
        /// Loads the data file.
        /// </summary>
        private void LoadDataFile(List<DirectoryComputerItem> list)
        {
            _directoryComputerCollections = list ?? new List<DirectoryComputerItem>();
            var root = _directoryComputerCollections.FirstOrDefault(c => c.Level == 0);
            if (root != null)
            {
                string[] arrImport = File.ReadAllLines(_filePath, Encoding.UTF8);
                foreach (var line in arrImport)
                {
                    if (!_isValid)
                    {
                        break;
                    }
                    if (line.Length > 0 && line[0].Equals(SPLICE_CHAR))
                    {
                        BuildTree(root.Id, root.Name,root.Name, line.Split(new [] { SPLICE_CHAR }), 1);
                    }
                    else
                    {
                        _isValid = false;
                        break;//Invalid line
                    }
                }
                if (!_isValid)
                {
                    _directoryComputerCollections.Clear();
                }
            }
        }
    }
}
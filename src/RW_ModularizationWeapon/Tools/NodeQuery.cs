using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace RW_ModularizationWeapon.Tools
{
    public class VNode : IEnumerable<VNode>
    {
        public VNode(string id, string defName, VNode parent = null)
        {
            this.id = id;
            this.defName = defName;
            this.parent = parent;
            if (parent != null) parent[id] = this;
        }

        public VNode this[string id]
        {
            get => child.TryGetValue(id);
            private set => child.SetOrAdd(id, value);
        }

        public readonly string id = null;
        public readonly string defName = null;
        public readonly VNode parent = null;
        private readonly Dictionary<string, VNode> child = new Dictionary<string, VNode>();

        public IEnumerator<VNode> GetEnumerator()
        {
            foreach (VNode node in child.Values) yield return node;
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class QueryGroup
    {
        public QueryGroup(){}

        public QueryGroup(string queryStr) => UpdateByQueryString(queryStr);

        public uint ConditionCount
        {
            get
            {
                uint result = 0;
                foreach (QueryLevel level in levels) result += level.ConditionCount;
                return result;
            }
        }

        public IEnumerable<string> AllAnnouncedTargetId
        {
            get
            {
                foreach(QueryLevel level in levels) yield return level.TargetId;
                yield break;
            }
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1) throw new Exception("Misconfigured ThingDefCountRangeClass: " + xmlRoot.OuterXml);
            UpdateByQueryString(xmlRoot.FirstChild.Value);
        }

        private void UpdateByQueryString(string queryStr)
        {
            int squareBracket = 0;
            int lastComma = 0;
            List<string> subLevels = new List<string>();
            for (int i = 0; i < queryStr.Length; i++)
            {
                switch (queryStr[i])
                {
                    case '[':
                    squareBracket++;
                    break;
                    case ']':
                    squareBracket--;
                    break;
                    case ',':
                    if(squareBracket == 0)
                    {
                        subLevels.Add(queryStr.Substring(lastComma, i - lastComma));
                        lastComma = i + 1;
                    }
                    break;
                }
                if (squareBracket < 0) throw new Exception("Invalidity query string");
            }
            subLevels.Add(queryStr.Substring(lastComma, queryStr.Length - lastComma));
            if (squareBracket != 0) throw new Exception("Invalidity query string");
            foreach (string query in subLevels)
            {
                if (query.Length > 0) levels.Add(new QueryLevel(query));
            }
        }

        public uint Mach(VNode node)
        {
            uint result = 0;
            foreach(QueryLevel level in levels)
            {
                result = Math.Max(result,level.Mach(node));
            }
            return result;
        }

        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < levels.Count; i++)
            {
                string query = levels[i].ToString();
                if (query.Length > 0) result += i == 0 ? query : ("," + query);
            }
            return result;
        }

        private List<QueryLevel> levels = new List<QueryLevel>();
    }
    public class QueryLevel
    {
        public QueryLevel(){}

        public QueryLevel(string queryStr) => UpdateByQueryString(queryStr);

        public uint ConditionCount
        {
            get
            {
                uint result = 0;
                foreach (QuerySelecter selecter in selecters) result += selecter.ConditionCount;
                return result;
            }
        }

        public string TargetId => selecters.NullOrEmpty() ? null : selecters[selecters.Count - 1].TargetId;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1) throw new Exception("Misconfigured ThingDefCountRangeClass: " + xmlRoot.OuterXml);
            UpdateByQueryString(xmlRoot.FirstChild.Value);
        }

        private void UpdateByQueryString(string queryStr)
        {
            int squareBracket = 0;
            int lastSlash = 0;
            List<string> subSelecter = new List<string>();
            for (int i = 0; i < queryStr.Length; i++)
            {
                switch (queryStr[i])
                {
                    case '[':
                    squareBracket++;
                    break;
                    case ']':
                    squareBracket--;
                    break;
                    case '/':
                    if(squareBracket == 0)
                    {
                        subSelecter.Add(queryStr.Substring(lastSlash, i - lastSlash));
                        lastSlash = i + 1;
                    }
                    break;
                }
                if (squareBracket < 0) throw new Exception("Invalidity query string");
            }
            subSelecter.Add(queryStr.Substring(lastSlash, queryStr.Length - lastSlash));
            if(squareBracket != 0) throw new Exception("Invalidity query string");
            foreach (string query in subSelecter)
            {
                if (query.Length > 0) selecters.Add(new QuerySelecter(query));
            }
        }

        public uint Mach(VNode node)
        {
            uint result = 0;
            for(int i = selecters.Count - 1; i >= 0; i--)
            {
                if (node == null) return 0;
                uint mach = selecters[i].Mach(node);
                if (mach == 0) return 0;
                result += mach;
                node = node.parent;
            }
            return result;
        }

        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < selecters.Count; i++)
            {
                string query = selecters[i].ToString();
                if (query.Length > 0) result += i == 0 ? query : ("/" + query);
            }
            return result;
        }

        private List<QuerySelecter> selecters = new List<QuerySelecter>();
    }
    public class QuerySelecter
    {
        public QuerySelecter(){}
        public QuerySelecter(string queryStr) => UpdateByQueryString(queryStr);

        public uint ConditionCount
        {
            get
            {
                uint result = 0;
                if (notFlag) result++;
                if (id != null) result++;
                if (defName != null) result++;
                foreach (QueryGroup group in childenGroups) result += group.ConditionCount;
                return result;
            }
        }

        public string TargetId => id;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1) throw new Exception("Misconfigured ThingDefCountRangeClass: " + xmlRoot.OuterXml);
            UpdateByQueryString(xmlRoot.FirstChild.Value);
        }

        private void UpdateByQueryString(string queryStr)
        {
            if (queryStr.StartsWith("!")) notFlag = true;
            int squareBracket = 0;
            int lastSquareBracketStart = 0;
            int colonStart = 0;
            int idStart = -1;
            List<string> squareBracketPacked = new List<string>();
            for(int i = notFlag ? 1 : 0; i < queryStr.Length; i++)
            {
                switch (queryStr[i])
                {
                    case '[':
                    {
                        if (squareBracket == 0)
                        {
                            if (colonStart > 0 && defName == null) defName = queryStr.Substring(colonStart, i - colonStart);
                            else if (idStart >= 0 && id == null) id = queryStr.Substring(idStart,i - idStart);
                            lastSquareBracketStart = i + 1;
                        }
                        squareBracket++;
                        break;
                    }
                    case ']':
                    {
                        squareBracket--;
                        if (squareBracket == 0) squareBracketPacked.Add(queryStr.Substring(lastSquareBracketStart, i - lastSquareBracketStart));
                        break;
                    }
                    case ':':
                    {
                        if (squareBracket == 0)
                        {
                            if (idStart >= 0 && id == null) id = queryStr.Substring(idStart,i - idStart);
                            colonStart = i + 1;
                        }
                        break;
                    }
                    case '!':
                    {
                        if (squareBracket == 0) throw new Exception($"Invalidity query string at {queryStr.Substring(i)}");
                        break;
                    }
                    default:
                    {
                        if(squareBracket == 0 && (colonStart == 0 || defName != null))
                        {
                            if (id != null) throw new Exception($"Invalidity query string at {queryStr.Substring(i)}");
                            if (idStart < 0) idStart = i;
                        }
                        break;
                    }
                }
                if(squareBracket < 0) throw new Exception($"Invalidity query string at {queryStr.Substring(i)}");
            }
            if(squareBracket == 0)
            {
                if (colonStart > 0 && defName == null) defName = queryStr.Substring(colonStart, queryStr.Length - colonStart);
                else if (idStart >= 0 && id == null) id = queryStr.Substring(idStart,queryStr.Length - idStart);
            }
            else throw new Exception("Invalidity query string");
            foreach(string query in squareBracketPacked)
            {
                if (query.Length > 0) childenGroups.Add(new QueryGroup(query));
            }
        }

        public uint Mach(VNode node)
        {
            if(node == null) return 0;
            uint result = notFlag ? 1u : 0;
            if(id != null)
            {
                if (node.id == id && !notFlag) result++;
                else return 0;
            }
            if(defName != null)
            {
                if (node.defName == defName && !notFlag) result++;
                else return 0;
            }
            foreach(QueryGroup group in childenGroups)
            {
                uint mach = 0;
                foreach(VNode child in node) mach = Math.Max(mach,group.Mach(child));
                if (notFlag)
                {
                    if(mach > 0) return 0;
                    else mach = group.ConditionCount;
                }
                else if(mach == 0) return 0;
                result += mach;
            }
            return result;
        }

        public override string ToString()
        {
            string result = notFlag ? $"!{id}:{defName}" : $"{id}:{defName}";
            foreach(QueryGroup query in childenGroups)
            {
                result += $"[{query}]";
            }
            return result;
        } 

        private bool notFlag = false;
        private string id = null;
        private string defName = null;
        private List<QueryGroup> childenGroups = new List<QueryGroup>();

    }
}

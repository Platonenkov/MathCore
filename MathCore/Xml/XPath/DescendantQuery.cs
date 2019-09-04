// ReSharper disable once CheckNamespace
namespace System.Xml.XPath
{
    internal class DescendantQuery : BaseAxisQuery
    {
        #region Constructors

        public DescendantQuery(Query QyInput, string name, string prefix, XPathNodeType type) : base(QyInput, name, prefix, type) { }

        #endregion

        #region Methods

        internal override bool MatchNode(XPathReader reader)
        {
            var ret = true;

            if(NodeType == XPathNodeType.All) return ret;
            if(!MatchType(NodeType, reader.NodeType))
                ret = false;
            else if(Name != null && (Name != reader.Name || Prefix != reader.Prefix))
                ret = false;

            return ret;
        }


        //
        // Desendant query value
        //
        // <e><e1>1</e1><e2>2</e2></e>
        //
        // e[desendant::node()=1]
        //
        // current context node need to be saved if
        // we need to solve the case for future.
        /// <exception cref="XPathReaderException">Can't get the decendent nodes value</exception>
        internal override object GetValue(XPathReader reader) => throw new XPathReaderException("Can't get the decendent nodes value");

        #endregion
    }
}
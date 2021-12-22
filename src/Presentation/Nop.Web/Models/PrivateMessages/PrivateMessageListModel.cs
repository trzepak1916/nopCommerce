using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.PrivateMessages
{
    public partial record PrivateMessageListModel : BaseNopModel
    {
        public IList<PrivateMessageModel> Messages { get; set; }
        public PagerModel PagerModel { get; set; }
    }
}
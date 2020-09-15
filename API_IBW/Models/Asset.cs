using API_IBW.DB_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_IBW.Models
{
    public class AssetModel
    {
        public List<GetAssetsResult> Asset { get; set; }
        public List<GetAssetCategoriesResult> Categories { get; set; }
    }
}
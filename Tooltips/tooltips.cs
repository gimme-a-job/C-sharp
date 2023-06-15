        #region パラメータ

        #region Const


        #region 営業部署の列挙型 // 2023.05.16 追加
        // (現状使わない)
        enum Dept
        {
            Rcpt = 0,
            Sales = 1
        }

        #endregion 営業部署の列挙型

        #region リソースキー // 2023.05.16 追加

        /// <summary>
        /// "Common"リソースファイル
        /// </summary>
        private const string resxCommon = "Common";

        /// <summary>
        /// タイトル書式
        /// </summary>
        private const string Text_labelBarTitle = "Text_labelBarTitle";

        /// <summary>
        /// 対応部署
        /// </summary>
        private const string Text_header_DeptInChrg = "Text_header_DeptInChrg";

        /// <summary>
        /// 受付
        /// </summary>
        private const string Text_DeptInChrg_Rcpt = "Text_DeptInChrg_Rcpt";

        /// <summary>
        /// 営業部署
        /// </summary>
        private const string Text_DeptInChrg_Sales = "Text_DeptInChrg_Sales";


        #region 出迎え関係

        /// <summary>
        /// 出迎えありかどうか
        /// </summary>
        private const string Text_header_PickUp = "Text_header_PickUp";

        /// <summary>
        /// 出迎え場所
        /// </summary>
        private const string Text_header_PickUp_PckUpFrm = "Text_header_PickUp_PckUpFrm";

        /// <summary>
        /// 出迎え担当者 名前
        /// </summary>
        private const string Text_header_PickUp_PersInChrg_Name = "Text_header_PickUp_PersInChrg_Name";

        /// <summary>
        /// 出迎え担当者 電話番号
        /// </summary>
        private const string Text_header_PickUp_PersInChrg_Tel = "Text_header_PickUp_PersInChrg_Tel";

        #endregion 出迎え関係

        #region お茶出し関係

        /// <summary>
        /// お茶出しありかどうか
        /// </summary>
        private const string Text_header_DrinkSrv = "Text_header_DrinkSrv";
      
        /// <summary>
        /// お茶出し担当者 名前
        /// </summary>
        private const string Text_header_DrinkSrv_PersInChrg_Name = "Text_header_DrinkSrv_PersInChrg_Name";

        /// <summary>
        /// お茶出し担当者 電話番号
        /// </summary>
        private const string Text_header_DrinkSrv_PersInChrg_Tel = "Text_header_DrinkSrv_PersInChrg_Tel";

        /// <summary>
        /// 給茶メニュー
        /// </summary>
        private const string Text_header_DrinkSrv_DrinkMenu = "Text_header_DrinkSrv_DrinkMenu";

        /// <summary>
        /// 給茶個数
        /// </summary>
        private const string Text_header_DrinkSrv_DrinkCnt = "Text_header_DrinkSrv_DrinkCnt";

        #endregion お茶出し関係

        #region 見送り関係

        /// <summary>
        /// 見送りありかどうか
        /// </summary>
        private const string Text_header_SendOff = "Text_header_SendOff";

        /// <summary>
        /// 見送り場所
        /// </summary>
        private const string Text_header_SendOff_SndOffTo = "Text_header_SendOff_SndOffTo";

        /// <summary>
        /// 見送り担当者 名前
        /// </summary>
        private const string Text_header_SendOff_PersInChrg_Name = "Text_header_SendOff_PersInChrg_Name";

        /// <summary>
        /// 見送り担当者 電話番号
        /// </summary>
        private const string Text_header_SendOff_PersInChrg_Tel = "Text_header_SendOff_PersInChrg_Tel";

        #endregion 見送り関係

        #endregion リソースキー

        #endregion Const

        #endregion パラメータ


        /// <summary>
        /// リソースキーに対応したタイトル文字列(---{タイトル}---------)をstring型で取得する。 // 2023.05.16 追加
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        private string getTitleFrmResx(string resourceKey)
        {
            // 言語設定の取得
            var culture = Session["Culture"].ToString();
            // タイトルが２バイト文字かどうか
            var isDblB = true;
            if (!culture.Equals("ja-JP")) isDblB = false;

            // タイトル書式を取得
            var ttlFmt = getTxtFrmResx(resourceKey: Text_labelBarTitle);
            // リソースキーからタイトルを取得
            var title = getTxtFrmResx(resourceKey: resourceKey);

            // 削る文字数の判定
            var numTrim = 0;
            if (isDblB) numTrim = (2 * (title.Length - 2)); // 最後の"2"は「メモ」の文字数
            else numTrim = (1 * (title.Length - (4+2))); // 後ろの"4"は"Memo"の文字数…のつもりだったが合わないためさらに"+2"

            // タイトル書式のトリム
            if (numTrim > 0) ttlFmt = ttlFmt.Remove(ttlFmt.Length - numTrim);

            // 戻り値の設定
            var strMes = string.Format(ttlFmt, title);

            return strMes;
        }

        /// <summary>
        /// リソースキーに対応した文字列をstring型で取得する。 // 2023.05.16 追加
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        private string getTxtFrmResx(string resourceKey, string argScreenId = ScreenId)
        {
            return HttpContext.GetGlobalResourceObject(argScreenId, resourceKey).ToString();
        }

        /// <summary>
        /// 数字(文字列)を対応部署名に変換する // 2023.05.16 追加
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        private string getDeptInChrg(object obj)
        {
            var retStr = "";

            var deptVal = GetDataRowColumnToString(obj);

            if (!String.IsNullOrEmpty(deptVal))

                switch (deptVal)
                {
                    // 受付
                    case "0":
                        retStr = getTxtFrmResx(resourceKey: Text_DeptInChrg_Rcpt, argScreenId: resxCommon).Split('<')[0]; // 現状"<span～"を入れてしまっているため
                        break;
                    // 営業部署
                    case "1":
                        retStr = getTxtFrmResx(resourceKey: Text_DeptInChrg_Sales, argScreenId: resxCommon);
                        break;
                    // (部署名が増える場合、この辺に追加する必要が出てきます。。)
                    default:
                        // 何もしない
                        break;
                }

            return retStr;
        }


        private string getMainToolTip(DataRowView dr)
        {            
            StringBuilder sb = new StringBuilder();

            var arrStr = new List<string>(){
                
                //お客様会社名
                HttpContext.GetGlobalResourceObject(ScreenId, "Text_Header_Company_Name").ToString(), Colon, GetDataRowColumnToString(dr[DataRow_Visitor_Company_Name]), LineBreak,
                                
                //お客様名（カナ）"                
                HttpContext.GetGlobalResourceObject(ScreenId, "Text_header_VisitorName_Kana").ToString(), Colon, GetDataRowColumnToString(dr[DataRow_Visitor_Name_Kana]), LineBreak,
                
                //"--- 伝言 -----------------------"
                getTitleFrmResx(resourceKey: "Text_header_Note"), LineBreak,
                GetDataRowColumnToString(dr[DataRow_Visitor_Remarks]), LineBreak,
                
                //"--- メモ ---------------------"
                HttpContext.GetGlobalResourceObject(ScreenId, "Text_labelBarTitleMemo").ToString(), LineBreak,
                GetDataRowColumnToString(dr[DataRow_Reception_Remarks]), LineBreak,
                
                LineBreak

            };

            #region 「出迎え」
            // "出迎え"
            if (Boolean.Parse(GetDataRowColumnToString(dr[DataRow_Reserve_IsPickUp])))
            {
                var arrPck = new List<string>()
                {
                    getTitleFrmResx(resourceKey: Text_header_PickUp), LineBreak,

                    // 担当部署
                    getTxtFrmResx(resourceKey: Text_header_DeptInChrg), Colon, getDeptInChrg(dr[DataRow_Reserve_PickUp_DeptInChrg]), LineBreak,

                    // 出迎え場所
                    getTxtFrmResx(resourceKey: Text_header_PickUp_PckUpFrm), Colon, GetDataRowColumnToString(dr[DataRow_Reserve_PickUp_PckUpFrm]), LineBreak,
                };

                // 「出迎え担当者 氏名」を先に取得
                var name = GetDataRowColumnToString(dr[DataRow_Reserve_PickUp_PersInChrg_Name]);

                // 出迎え担当者名が取得されているかどうかで、表示・非表示を決める
                if (!String.IsNullOrEmpty(name)) arrPck.AddRange(new List<string>()
                {
                    // 出迎え担当者 氏名
                    getTxtFrmResx(resourceKey: Text_header_PickUp_PersInChrg_Name), Colon, name, LineBreak,

                    // 出迎え担当者 Tel
                    getTxtFrmResx(resourceKey: Text_header_PickUp_PersInChrg_Tel), Colon, GetDataRowColumnToString(dr[DataRow_Reserve_PickUp_PersInChrg_Tel]), LineBreak,
                });

                // 空行の追加
                arrPck.Add(LineBreak);

                arrStr.AddRange(arrPck);
            }
            #endregion "出迎え"

            #region 「お茶出し」
            // "お茶出し"
            if (Boolean.Parse(GetDataRowColumnToString(dr[DataRow_Reserve_IsDrinkSrv])))
            {
                var arrDrk = new List<string>()
                {
                    getTitleFrmResx(resourceKey: Text_header_DrinkSrv), LineBreak,

                    // 担当部署
                    getTxtFrmResx(resourceKey: Text_header_DeptInChrg), Colon, getDeptInChrg(dr[DataRow_Reserve_DrinkSrv_DeptInChrg]), LineBreak,
                    
                };

                // 「お茶出し担当者 氏名」を先に取得
                var name = GetDataRowColumnToString(dr[DataRow_Reserve_DrinkSrv_PersInChrg_Name]);

                // お茶出し担当者名が取得されているかどうかで、表示・非表示を決める
                if (!String.IsNullOrEmpty(name)) arrDrk.AddRange(new List<string>()
                {
                    // お茶出し担当者 氏名
                    getTxtFrmResx(resourceKey: Text_header_DrinkSrv_PersInChrg_Name), Colon, name, LineBreak,

                    // お茶出し担当者 Tel
                    getTxtFrmResx(resourceKey: Text_header_DrinkSrv_PersInChrg_Tel), Colon, GetDataRowColumnToString(dr[DataRow_Reserve_DrinkSrv_PersInChrg_Tel]), LineBreak,
                });

                arrDrk.AddRange(new List<string>()
                {
                    // 給茶メニュー
                    getTxtFrmResx(resourceKey: Text_header_DrinkSrv_DrinkMenu), Colon, GetDataRowColumnToString(dr[DataRow_Reserve_DrinkSrv_DrinkMenu]), LineBreak,

                    // 個数
                    getTxtFrmResx(resourceKey: Text_header_DrinkSrv_DrinkCnt), Colon, GetDataRowColumnToString(dr[DataRow_Reserve_DrinkSrv_DrinkCnt]), LineBreak,

                    // 空行
                    LineBreak
            });                

                arrStr.AddRange(arrDrk);
            }
            #endregion "お茶出し"

            #region 「見送り」
            // "見送り"
            if (Boolean.Parse(GetDataRowColumnToString(dr[DataRow_Reserve_IsSendOff])))
            {
                var arrSnd = new List<string>()
                {
                    getTitleFrmResx(resourceKey: Text_header_SendOff), LineBreak,

                    // 担当部署
                    getTxtFrmResx(resourceKey: Text_header_DeptInChrg), Colon, getDeptInChrg(dr[DataRow_Reserve_SendOff_DeptInChrg]), LineBreak,

                    // 見送り場所
                    getTxtFrmResx(resourceKey: Text_header_SendOff_SndOffTo), Colon, GetDataRowColumnToString(dr[DataRow_Reserve_SendOff_SndOffTo]), LineBreak,
                };

                // 「見送り担当者 氏名」を先に取得
                var name = GetDataRowColumnToString(dr[DataRow_Reserve_SendOff_PersInChrg_Name]);

                // 見送り担当者名が取得されているかどうかで、表示・非表示を決める
                if (!String.IsNullOrEmpty(name)) arrSnd.AddRange(new List<string>()
                {
                    // 見送り担当者 氏名
                    getTxtFrmResx(resourceKey: Text_header_SendOff_PersInChrg_Name), Colon, name, LineBreak,

                    // 見送り担当者 Tel
                    getTxtFrmResx(resourceKey: Text_header_SendOff_PersInChrg_Tel), Colon, GetDataRowColumnToString(dr[DataRow_Reserve_SendOff_PersInChrg_Tel]), LineBreak,
                });

                // 空行の追加
                arrSnd.Add(LineBreak);

                arrStr.AddRange(arrSnd);
            }
            #endregion "見送り"

            //"--------------------------------"
            arrStr.Add(HttpContext.GetGlobalResourceObject(ScreenId, "Text_labelBarEnd").ToString());

            // List内の<string>を順に結合
            foreach (string str in arrStr) sb.Append(str);

            return sb.ToString();
        }
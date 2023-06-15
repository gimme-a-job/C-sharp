        #region 予約情報への登録・更新関係 // 2023.05.12 追加

        /// <summary>
        /// パラメータのクラス // 2023.05.12 追加
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        protected class Params
        {
            public string pName { get; set; } // パラメータ名
            public string value { get; set; } // 登録・更新する値
            public SqlDbType sqlDbType { get; set; }

            // コンストラクタ
            public Params(string pName, string value, SqlDbType sqlDbType)
            {
                this.pName = pName;
                this.value = value;
                this.sqlDbType = sqlDbType;
            }
        }

        /// <summary>
        /// パラメータの作成 // 2023.05.12 追加
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        // 一々"new"するよりは使いやすいかなと…
        private Params GetParams(string pName, string value, SqlDbType sqlDbType)
        {
            return new Params(pName: pName, value: value, sqlDbType: sqlDbType);
        }

        /// <summary>
        /// パラメータ名の定数を格納 // 2023.05.12 追加
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public abstract class Parameter
        {
            #region 「出迎え」

            /// <summary>
            /// 「出迎え」の「なし・あり」
            /// </summary>
            public const string IsPickUp = "@pIsPickUp";

            /// <summary>
            /// 担当部署
            /// </summary>
            public const string PickUp_Dept_In_Charge = "@pPickUp_Dept_In_Charge";

            /// <summary>
            /// 場所
            /// </summary>
            public const string PickUp_Pick_Up_From = "@pPickUp_Pick_Up_From";

            /// <summary>
            /// 担当者名前
            /// </summary>
            public const string PickUp_Pers_In_Charge_Name = "@pPickUp_Pers_In_Charge_Name";

            /// <summary>
            /// 担当者電話番号
            /// </summary>
            public const string PickUp_Pers_In_Charge_Tel = "@pPickUp_Pers_In_Charge_Tel";

            #endregion

            #region 「お茶出し」
            /// <summary>
            /// 「お茶出し」の「なし・あり」
            /// </summary>
            public const string IsDrinkSrv = "@pIsDrinkSrv";

            /// <summary>
            /// 担当部署
            /// </summary>
            public const string DrinkSrv_Dept_In_Charge = "@pDrinkSrv_Dept_In_Charge";

            /// <summary>
            /// 担当者名前
            /// </summary>
            public const string DrinkSrv_Pers_In_Charge_Name = "@pDrinkSrv_Pers_In_Charge_Name";

            /// <summary>
            /// 担当者電話番号
            /// </summary>
            public const string DrinkSrv_Pers_In_Charge_Tel = "@pDrinkSrv_Pers_In_Charge_Tel";

            /// <summary>
            /// お出しする飲み物
            /// </summary>
            public const string DrinkSrv_Drink_Menu = "@pDrinkSrv_Drink_Menu";

            /// <summary>
            /// 飲み物の数
            /// </summary>
            public const string DrinkSrv_Drink_Cnt = "@pDrinkSrv_Drink_Cnt";

            #endregion

            #region 「見送り」

            /// <summary>
            /// 「見送り」の「なし・あり」
            /// </summary>
            public const string IsSendOff = "@pIsSendOff";

            /// <summary>
            /// 担当部署
            /// </summary>
            public const string SendOff_Dept_In_Charge = "@pSendOff_Dept_In_Charge";

            /// <summary>
            /// 場所
            /// </summary>
            public const string SendOff_Send_Off_To = "@pSendOff_Send_Off_To";

            /// <summary>
            /// 担当者名前
            /// </summary>
            public const string SendOff_Pers_In_Charge_Name = "@pSendOff_Pers_In_Charge_Name";

            /// <summary>
            /// 担当者電話番号
            /// </summary>
            public const string SendOff_Pers_In_Charge_Tel = "@pSendOff_Pers_In_Charge_Tel";

            #endregion
        }

        /// <summary>
        /// パラメータのセット // 2023.05.12 追加
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        protected void setSQLParameters(ref SqlCommand sql, IEnumerable<Params> @params)
        {
            foreach (Params p in @params) setSqlParameter(ref sql, p.pName, p.value, p.sqlDbType);
        }

                /// <summary>
        /// 「出迎え」パラメータのセット // 2023.05.12 追加
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private void setSQLParameters_PickUp(ref SqlCommand sqlCommand, bool isPickUp = true)
        {

            #region 出迎え「あり」
            if (isPickUp && "1".Equals(RadioButtonList_isPickUp_VIP.SelectedValue))
            {
                setSQLParameters(sql: ref sqlCommand, @params: new List<Params>() {

                                            // 「出迎え」を「あり」にセット
                                            GetParams(
                                                Parameter.IsPickUp,
                                                "1",
                                                SqlDbType.Bit
                                                ),

                                            // 担当部署
                                            GetParams(
                                                Parameter.PickUp_Dept_In_Charge,
                                                RadioButtonList_PickUp_DeptInChrg_VIP.SelectedValue,
                                                SqlDbType.Int
                                                ),

                                            // 出迎え場所
                                            GetParams(
                                                Parameter.PickUp_Pick_Up_From,
                                                DropDownList_PickUp_PckUpFrm_VIP.SelectedValue,
                                                SqlDbType.Int // 文字列がそのまま来るとかであれば変更の必要あり
                                                )

                                            });

                #region 担当者部署チェック

                #region 担当部署が「受付」以外
                if (!("0".Equals(RadioButtonList_PickUp_DeptInChrg_VIP.SelectedValue)))
                {
                    setSQLParameters(sql: ref sqlCommand, @params: new List<Params>() {

                                                // 出迎え担当者 名前
                                                GetParams(
                                                    Parameter.PickUp_Pers_In_Charge_Name,
                                                    TextBox_PickUp_PersInChrg_Name_VIP.Text,
                                                    SqlDbType.NVarChar
                                                    ),

                                                // 出迎え担当者 Tel
                                                GetParams(
                                                    Parameter.PickUp_Pers_In_Charge_Tel,
                                                    TextBox_PickUp_PersInChrg_Tel_VIP.Text,
                                                    SqlDbType.NVarChar
                                                    )

                                            });
                }
                #endregion 担当部署が「受付」以外

                #region 担当部署が「受付」
                else
                {
                    setSQLParameters(sql: ref sqlCommand, @params: new List<Params>() {

                                                // 出迎え担当者 名前
                                                GetParams(
                                                    Parameter.PickUp_Pers_In_Charge_Name,
                                                    "",
                                                    SqlDbType.NVarChar
                                                    ),

                                                // 出迎え担当者 Tel
                                                GetParams(
                                                    Parameter.PickUp_Pers_In_Charge_Tel,
                                                    "",
                                                    SqlDbType.NVarChar
                                                    )

                                            });
                }
                #endregion 担当部署が「受付」

                #endregion 担当部署チェック

            }
            #endregion 出迎え「あり」

            #region 出迎え「なし」
            else
            {
                setSQLParameters(sql: ref sqlCommand, @params: new List<Params>() {

                                            // 「出迎え」を「あり」にセット
                                            GetParams(
                                                Parameter.IsPickUp,
                                                "0",
                                                SqlDbType.Bit
                                                ),

                                            // 担当部署
                                            GetParams(
                                                Parameter.PickUp_Dept_In_Charge,
                                                null,
                                                SqlDbType.Int
                                                ),

                                            // 出迎え場所
                                            GetParams(
                                                Parameter.PickUp_Pick_Up_From,
                                                null, // 文字列がそのまま来るとかであれば変更の必要ありかも
                                                SqlDbType.Int // 文字列がそのまま来るとかであれば変更の必要ありかも
                                                ),

                                            // 出迎え担当者 名前
                                            GetParams(
                                                Parameter.PickUp_Pers_In_Charge_Name,
                                                "",
                                                SqlDbType.NVarChar
                                                ),
                                            
                                            // 出迎え担当者 Tel                                            
                                            GetParams(
                                                Parameter.PickUp_Pers_In_Charge_Tel,
                                                "",
                                                SqlDbType.NVarChar
                                                )

                                        });
            }
            #endregion 出迎え「なし」

        }

        /// <summary>
        /// 「お茶出し」パラメータのセット // 2023.05.12 追加
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private void setSQLParameters_DrinkSrv(ref SqlCommand sqlCommand, bool isDrinkSrv = true)
        {

            #region お茶出し「あり」
            if (isDrinkSrv && "1".Equals(RadioButtonList_isDrinkSrv_VIP.SelectedValue))
            {
                setSQLParameters(sql: ref sqlCommand, @params: new List<Params>() {

                                            // 「お茶出し」を「あり」にセット
                                            GetParams(
                                                Parameter.IsDrinkSrv,
                                                "1",
                                                SqlDbType.Bit
                                                ),

                                            // 担当部署
                                            GetParams(
                                                Parameter.DrinkSrv_Dept_In_Charge,
                                                RadioButtonList_DrinkSrv_DeptInChrg_VIP.SelectedValue,
                                                SqlDbType.Int
                                                ),

                                            // 給茶メニュー
                                            GetParams(
                                                Parameter.DrinkSrv_Drink_Menu,
                                                DropDownList_DrinkSrv_DrinkMenu_VIP.SelectedValue,
                                                SqlDbType.Int // 文字列がそのまま来るとかであれば変更の必要あり
                                                ),

                                            // 個数
                                            GetParams(
                                                Parameter.DrinkSrv_Drink_Cnt,
                                                TextBox_DrinkSrv_DrinkCnt_VIP.Text,
                                                SqlDbType.Int
                                                )

                                            });

                #region 担当者部署チェック

                #region 担当部署が「受付」以外
                if (!("0".Equals(RadioButtonList_DrinkSrv_DeptInChrg_VIP.SelectedValue)))
                {
                    setSQLParameters(sql: ref sqlCommand, @params: new List<Params>() {

                                                // お茶出し担当者 名前
                                                GetParams(
                                                    Parameter.DrinkSrv_Pers_In_Charge_Name,
                                                    TextBox_DrinkSrv_PersInChrg_Name_VIP.Text,
                                                    SqlDbType.NVarChar
                                                    ),

                                                // お茶出し担当者 Tel
                                                GetParams(
                                                    Parameter.DrinkSrv_Pers_In_Charge_Tel,
                                                    TextBox_DrinkSrv_PersInChrg_Tel_VIP.Text,
                                                    SqlDbType.NVarChar
                                                    )

                                            });
                }
                #endregion 担当部署が「受付」以外

                #region 担当部署が「受付」
                else
                {
                    setSQLParameters(sql: ref sqlCommand, @params: new List<Params>() {

                                                // お茶出し担当者 名前
                                                GetParams(
                                                    Parameter.DrinkSrv_Pers_In_Charge_Name,
                                                    "",
                                                    SqlDbType.NVarChar
                                                    ),

                                                // お茶出し担当者 Tel
                                                GetParams(
                                                    Parameter.DrinkSrv_Pers_In_Charge_Tel,
                                                    "",
                                                    SqlDbType.NVarChar
                                                    )

                                            });
                }
                #endregion 担当部署が「受付」

                #endregion 担当部署チェック

            }
            #endregion お茶出し「あり」

            #region お茶出し「なし」
            else
            {
                setSQLParameters(sql: ref sqlCommand, @params: new List<Params>() {

                                            // 「お茶出し」を「あり」にセット
                                            GetParams(
                                                Parameter.IsDrinkSrv,
                                                "0",
                                                SqlDbType.Bit
                                                ),

                                            // 担当部署
                                            GetParams(
                                                Parameter.DrinkSrv_Dept_In_Charge,
                                                null,
                                                SqlDbType.Int
                                                ),

                                            // お茶出し担当者 名前
                                            GetParams(
                                                Parameter.DrinkSrv_Pers_In_Charge_Name,
                                                "",
                                                SqlDbType.NVarChar
                                                ),
                                            
                                            // お茶出し担当者 Tel                                            
                                            GetParams(
                                                Parameter.DrinkSrv_Pers_In_Charge_Tel,
                                                "",
                                                SqlDbType.NVarChar
                                                ),

                                            // 給茶メニュー
                                            GetParams(
                                                Parameter.DrinkSrv_Drink_Menu,
                                                null, // 文字列がそのまま来るとかであれば変更の必要ありかも
                                                SqlDbType.Int // 文字列がそのまま来るとかであれば変更の必要ありかも
                                                ),

                                            // 個数
                                            GetParams(
                                                Parameter.DrinkSrv_Drink_Cnt,
                                                null,
                                                SqlDbType.Int
                                                )

                                        });
            }
            #endregion お茶出し「なし」

        }

        /// <summary>
        /// 「見送り」パラメータのセット // 2023.05.12 追加
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private void setSQLParameters_SendOff(ref SqlCommand sqlCommand, bool isSendOff = true)
        {

            #region 見送り「あり」
            if (isSendOff && "1".Equals(RadioButtonList_isSendOff_VIP.SelectedValue))
            {
                setSQLParameters(sql: ref sqlCommand, @params: new List<Params>() {

                                            // 「見送り」を「あり」にセット
                                            GetParams(
                                                Parameter.IsSendOff,
                                                "1",
                                                SqlDbType.Bit
                                                ),

                                            // 担当部署
                                            GetParams(
                                                Parameter.SendOff_Dept_In_Charge,
                                                RadioButtonList_SendOff_DeptInChrg_VIP.SelectedValue,
                                                SqlDbType.Int
                                                ),

                                            // 見送り場所
                                            GetParams(
                                                Parameter.SendOff_Send_Off_To,
                                                DropDownList_SendOff_SndOffTo_VIP.SelectedValue,
                                                SqlDbType.Int // 文字列がそのまま来るとかであれば変更の必要あり
                                                )

                                            });

                #region 担当者部署チェック

                #region 担当部署が「受付」以外
                if (!("0".Equals(RadioButtonList_SendOff_DeptInChrg_VIP.SelectedValue)))
                {
                    setSQLParameters(sql: ref sqlCommand, @params: new List<Params>() {

                                                // 見送り担当者 名前
                                                GetParams(
                                                    Parameter.SendOff_Pers_In_Charge_Name,
                                                    TextBox_SendOff_PersInChrg_Name_VIP.Text,
                                                    SqlDbType.NVarChar
                                                    ),

                                                // 見送り担当者 Tel
                                                GetParams(
                                                    Parameter.SendOff_Pers_In_Charge_Tel,
                                                    TextBox_SendOff_PersInChrg_Tel_VIP.Text,
                                                    SqlDbType.NVarChar
                                                    )

                                            });
                }
                #endregion 担当部署が「受付」以外

                #region 担当部署が「受付」
                else
                {
                    setSQLParameters(sql: ref sqlCommand, @params: new List<Params>() {

                                                // 見送り担当者 名前
                                                GetParams(
                                                    Parameter.SendOff_Pers_In_Charge_Name,
                                                    "",
                                                    SqlDbType.NVarChar
                                                    ),

                                                // 見送り担当者 Tel
                                                GetParams(
                                                    Parameter.SendOff_Pers_In_Charge_Tel,
                                                    "",
                                                    SqlDbType.NVarChar
                                                    )

                                            });
                }
                #endregion 担当部署が「受付」

                #endregion 担当部署チェック

            }
            #endregion 見送り「あり」

            #region 見送り「なし」
            else
            {
                setSQLParameters(sql: ref sqlCommand, @params: new List<Params>() {

                                            // 「見送り」を「あり」にセット
                                            GetParams(
                                                Parameter.IsSendOff,
                                                "0",
                                                SqlDbType.Bit
                                                ),

                                            // 担当部署
                                            GetParams(
                                                Parameter.SendOff_Dept_In_Charge,
                                                null,
                                                SqlDbType.Int
                                                ),

                                            // 見送り場所
                                            GetParams(
                                                Parameter.SendOff_Send_Off_To,
                                                null, // 文字列がそのまま来るとかであれば変更の必要ありかも
                                                SqlDbType.Int // 文字列がそのまま来るとかであれば変更の必要ありかも
                                                ),

                                            // 見送り担当者 名前
                                            GetParams(
                                                Parameter.SendOff_Pers_In_Charge_Name,
                                                "",
                                                SqlDbType.NVarChar
                                                ),
                                            
                                            // 見送り担当者 Tel                                            
                                            GetParams(
                                                Parameter.SendOff_Pers_In_Charge_Tel,
                                                "",
                                                SqlDbType.NVarChar
                                                )

                                        });
            }
            #endregion 見送り「なし」

        }

        #endregion 

        #region 予約情報からの内容復元関係 // 2023.05 追加

        /// <summary>
        /// 予約情報からラジオボタンの選択を復元する
        /// </summary>
        /// <returns></returns>
        private void getRadioButtonSelection(DataRow dr, string column, RadioButtonList radioButtonList, bool isBool = true)
        {
            if (isBool)
            {
                if (getDbBooleanNullFalse(dr[column]))
                {
                    radioButtonList.ClearSelection();

                    foreach (ListItem item in radioButtonList.Items)

                        if ("1".Equals(item.Value))
                        {
                            item.Selected = true;
                            break;
                        }
                    
                }
            }
            else radioButtonList.SelectedValue = Rsrv_Rgstr.getDbString(dr[column]);

        }

        /// <summary>
        /// 予約情報からドロップダウンの選択を復元する
        /// </summary>
        /// <returns></returns>
        private void getDropDownSelection(DataRow dr, string column, DropDownList dropDownList)
        {
            dropDownList.SelectedValue = Rsrv_Rgstr.getDbString(dr[column]);
            string buf = Rsrv_Rgstr.getDbString(dr[column]);
            if (!(string.IsNullOrEmpty(buf)))
            {
                dropDownList.ClearSelection();

                foreach (ListItem item in dropDownList.Items)

                    if (item.Value.Equals(buf))
                    {
                        item.Selected = true;
                        break;
                    }

            }
        }

        #endregion 予約情報からの内容復元時使用
        
        #region ビルオプション関係 // 2023.05 追加

        #region ラジオボタン関係

        /// <summary>
        /// ラジオボタンに「なし」「あり」をセットする。
        /// </summary>
        /// <returns></returns>
        protected void setYesOrNo(RadioButtonList radioButtonList, IEnumerable<string> argResourceKeys = null)
        {

            // デフォルト値、C#のコレクション型の順序が保障されている前提
            argResourceKeys = argResourceKeys ?? new List<string>() {
                "Text_No",
                "Text_Yes"
            };

            List<string> txts = getTxtsFrmResx(argResourceKeys);

            for (int i = 0; i < txts.Count; i++)
            {
                ListItem item;

                item = new ListItem();
                item.Text = txts[i];
                item.Value = i.ToString();
                if (i == 0) item.Selected = true;
                radioButtonList.Items.Add(item);
            }
        }

        #endregion ラジオボタン関係

        /// <summary>
        /// .resxファイルから文字列を取得 2023.05.22 追加
        /// </summary>
        /// <returns></returns>
        private string getTxtFrmResx(string argResourceKey)
        {
            return HttpContext.GetGlobalResourceObject("Common", argResourceKey).ToString();
        }

        /// <summary>
        /// .resxファイルから文字列を取得 2023.05.10 追加
        /// </summary>
        /// <returns></returns>
        private List<string> getTxtsFrmResx(IEnumerable<string> argResourceKeys)
        {
            return argResourceKeys
                .Select(resourceKey => getTxtFrmResx(argResourceKey: resourceKey))
                .ToList();
        }

        #region ドロップダウンリスト関係

        /// <summary>
        /// ドロップダウンリストのアイテム追加を行う([MST_BUILDING_OPTION]から) 2023.05.11 追加
        /// </summary>
        /// <returns></returns>
        private void setDropDownListFrmBldgOpt(string valueName, DataRow dr, IEnumerable<DropDownList> dropDownLists)
        {
            if (valueName.Equals(dr["VALUE_NAME"])) if (!(string.IsNullOrEmpty(dr["VALUE"].ToString()))) addItemsToDropDownList(
                dr: dr,
                argDropDownLists: dropDownLists,
                txtCol: "VALUE",
                valCol: "VALUE_ID"
                );
        }

        #region ドロップダウンリストへの任意のアイテム追加補助

        /// <summary>
        /// 1つのドロップダウンリストへアイテム追加を行う // 2023.05.16 追加
        /// </summary>
        /// <returns></returns>
        private void addItemtoDropDownList(DropDownList argDropDownList, string argTxt, string argVal)
        {
            ListItem listItem = new ListItem();
            listItem.Text = argTxt;
            listItem.Value = argVal;
            argDropDownList.Items.Add(listItem);
        }

        /// <summary>
        /// 複数のドロップダウンリストへアイテム追加を行う // 2023.05.16 追加
        /// </summary>
        /// <returns></returns>
        private void addItemtoDropDownList(IEnumerable<DropDownList> argDropDownLists, string argTxt, string argVal)
        {
            foreach (DropDownList dropDownList in argDropDownLists) addItemtoDropDownList(
                argDropDownList: dropDownList,
                argTxt: argTxt,
                argVal: argVal
                );
            
        }

        /// <summary>
        /// DataRowから１つのドロップダウンリストへアイテム追加を行う 2023.05.16 追加
        /// </summary>
        /// <returns></returns>
        private void addItemsToDropDownList(DataRow dr, DropDownList argDropDownList, string txtCol, string valCol)
        {
            addItemtoDropDownList(
                argDropDownList: argDropDownList,
                argTxt: dr[txtCol].ToString(),
                argVal: dr[valCol].ToString()
                );

        }

        /// <summary>
        /// DataRowから複数のドロップダウンリストへアイテム追加を行う 2023.05.15 追加
        /// </summary>
        /// <returns></returns>
        private void addItemsToDropDownList(DataRow dr, IEnumerable<DropDownList> argDropDownLists, string txtCol, string valCol)
        {
            addItemtoDropDownList(
                argDropDownLists: argDropDownLists,
                argTxt: dr[txtCol].ToString(),
                argVal: dr[valCol].ToString()
                );

        }

        #endregion ドロップダウンリストへの任意のアイテム追加

        /// <summary>
        /// １つのドロップダウンリストへアイテム追加を行う([SYS_DIVISION]から) 2023.05.16 追加
        /// </summary>
        /// <returns></returns>
        private void setDropDownListFrmSysDiv(string @pDivisionId, DropDownList argDropDownList, bool insBlankRow = false)
        {
            var dtSysDiv = GetDivision(
                DivisionId: @pDivisionId,
                Culture: getSessionValue(Session_Culture, Session)
                );

            // 空行の追加
            if (insBlankRow) addItemtoDropDownList(
                argDropDownList: argDropDownList,
                //argTxt: "",
                argTxt: getTxtFrmResx("Text_PlzSelect"), // 2023.05.22 追加
                argVal: "0"
                );

            foreach (DataRow dr in dtSysDiv.Rows) addItemsToDropDownList(
                         dr: dr,
                         argDropDownList: argDropDownList,
                         txtCol: MstSystemDivisionValueName,
                         valCol: MstSystemDivisionValue
                         );

        }

        /// <summary>
        /// 複数のドロップダウンリストへアイテム追加を行う([SYS_DIVISION]から) 2023.05.15 追加
        /// </summary>
        /// <returns></returns>
        private void setDropDownListFrmSysDiv(string @pDivisionId, IEnumerable<DropDownList> argDropDownLists, bool insBlankRow = false)
        {
            var dtSysDiv = GetDivision(
                DivisionId: @pDivisionId, 
                Culture: getSessionValue(Session_Culture, Session)
                );

            // 空行の追加
            if (insBlankRow) addItemtoDropDownList(
                argDropDownLists: argDropDownLists, 
                argTxt: "", 
                argVal: "0"
                );

            foreach (DataRow dr in dtSysDiv.Rows) addItemsToDropDownList(
                         dr: dr,
                         argDropDownLists: argDropDownLists,
                         txtCol: MstSystemDivisionValueName,
                         valCol: MstSystemDivisionValue
                         );

        }

        #endregion ドロップダウンリスト関係

        #endregion ビルオプション関係

        #region フロアオプション関係 // 2023.05.16 追加

        /// <summary>
        /// お茶出し表示可能なエリアかどうかのチェック 2023.05.16 追加
        /// </summary>
        /// <returns></returns>
        private bool isDrinkSrvFloor(string argFloorId)
        {
            var floorIds = System.Configuration.ConfigurationManager.AppSettings[Value_DrinkSrv_AreaId].Split(',');

            foreach (var floorId in floorIds) if (floorId.Equals(argFloorId)) return true;

            return false;
        }

        #region リテラル関係 2023.06.06 追加

        /// <summary>
        /// 見送りのリテラルへ文字列の設定を行う 2023.06.06 追加
        /// </summary>
        /// <returns></returns>
        private void setLiteralText_SndOff()
        {
            var rscKey =
                Panel_DrinkSrv_VIP.Visible ?
                // 給茶が表示されている場合
                "Text_PersInChrg_Ditto_PckDrk" // 「出迎え・お茶出し対応者と同じ場合は「同上」とご記入ください。」
                // 給茶が非表示の場合
                : "Text_PersInChrg_Ditto_Pck"; // 「出迎え対応者と同じ場合は「同上」とご記入ください。」

            // 見送り対応者欄の赤文字を設定する
            setLiteralText(
                argLiterals: new List<Literal>() {
                        Literal_SendOff_PersInChrg_Name_VIP,
                        Literal_SendOff_PersInChrg_Tel_VIP },
                argResouceKey: rscKey
                );

        }

        /// <summary>
        /// １つのリテラルへ文字列の設定を行う 2023.06.06 追加
        /// </summary>
        /// <returns></returns>
        private void setLiteralText(Literal argLiteral, string argResouceKey)
        {
            var txt = getTxtFrmResx(argResourceKey: argResouceKey);

            argLiteral.Text = txt;
        }

        /// <summary>
        /// 複数のリテラルへ文字列の設定を行う 2023.06.06 追加
        /// </summary>
        /// <returns></returns>
        private void setLiteralText(IEnumerable<Literal> argLiterals, string argResouceKey)
        {
            foreach (Literal literal in argLiterals) setLiteralText(
                argLiteral: literal,
                argResouceKey: argResouceKey
                );
        }

        #endregion リテラル関係
        // Web.configの設定取得用
        public const string Value_DrinkSrv_AreaId = "Value_DrinkSrv_AreaId"; // お茶出しを表示するエリアID
        
        /// <summary>
        /// フロアオプションの設定 // 2023.05.29 「お茶出し」表示・非表示不具合対応 追加
        /// </summary>
        protected void setFloorOption(Boolean isGuestEntry, string argFloor_Id)        
        {
            Boolean old_isViewGenOrVIP = Panel_isVIP.Visible; // 2023.06.13 追加
            Boolean old_isViewDrinkSrv = Panel_DrinkSrv_VIP.Visible;

            // 初期表示
            //Panel_isVIP.Visible = false; // 一般／VIP // やらない方が良さげ(31F⇒19F変更時に上手くない様子)
            Panel_DrinkSrv_VIP.Visible = false; // お茶出し(VIPのみ)
                       

            #region お茶出し

            // 現状"Panel_isVIP"(一般／VIP)の".visible"については"setBuildingOption()"でやってますので、
            // (続き) 今後「出迎え」「見送り」「駐車場」を全て非表示で「お茶出し」だけ表示する、みたいなことをやる場合は、
            // (続き) 今のままでは「お茶出し」が非表示になる可能性もあります。

            if (true) // フロアマスタに設定画面を付す場合、この辺りの条件詰める感じになると思います。
            {
                if (true) // フロアマスタに設定画面を付す場合、この辺りの条件詰める感じになると思います。
                {
                    #region 会議室予約なら
                    if (!isGuestEntry)
                    {
                        if (isDrinkSrvFloor(argFloor_Id))
                        {
                            Panel_isVIP.Visible = true;

                            Panel_DrinkSrv_VIP.Visible = true;
                        }

                    }
                    #endregion 会議室予約

                    #region 来訪者登録なら // 2023.05.24 追加
                    else
                    {
                        // 来訪者登録時にお茶出しを表示するかどうか
                        bool isDrinkSrv;
                        //// Web.configから設定をお取得（取得失敗時はfalse「表示しない」)）
                        //if (!Boolean.TryParse(ConfigurationManager.AppSettings[Value_GuestEntry_isDrinkSrv], out isDrinkSrv))
                        isDrinkSrv = false;
                        if (isDrinkSrv)
                        {
                            Panel_isVIP.Visible = true;

                            Panel_DrinkSrv_VIP.Visible = true;
                        }
                    }
                    #endregion 来訪者登録

                }
            }

            #endregion お茶出し


            #region 固定値ドロップダウン

            #region 一般／VIP (「お茶出し」のみ表示で、出迎え・見送り・駐車場がオフの場合に対応) 2023.06.13 追加
            if (!(IsPostBack && old_isViewGenOrVIP && Panel_isVIP.Visible)) // ← (「お茶出し」のみ表示で、出迎え・見送り・駐車場がオフの場合に対応)
            {
                RadioButtonList_isEvent.Items.Clear();

                setYesOrNo(
                    radioButtonList: RadioButtonList_isEvent,
                    argResourceKeys: new List<string>() {
                        "Text_NoApply","Text_Apply"
                    });

            }
            #endregion 一般／VIP

            #region お茶出し

            #region if条件(従来通り、「更新」以外はポストバック時に初期化させたい場合)
            //if
            //(
            //    modeNew ||
            //    modeRead ||
            //    ( modeEdit && !(IsPostBack && old_isViewDrinkSrv && Panel_DrinkSrv_VIP.Visible)) ||
            //    isGuestEntry
            //)
            #endregion if条件(従来通り、「更新」以外はポストバック時に初期化させたい場合)
            if (!(IsPostBack && old_isViewDrinkSrv && Panel_DrinkSrv_VIP.Visible)) // ← 入力済みの状態で会議室変更等をした際に、入力を残すため
            {
                DropDownList_DrinkSrv_DrinkMenu_VIP.Items.Clear(); // 給茶メニュー(VIPのみ)

                RadioButtonList_isDrinkSrv_VIP.Items.Clear(); // お茶出しするかどうか(VIPのみ)
                RadioButtonList_DrinkSrv_DeptInChrg_VIP.Items.Clear(); // お茶出し担当部署(VIPのみ)

                if (Panel_DrinkSrv_VIP.Visible)
                {
                    // 出迎え要／不要
                    setYesOrNo(RadioButtonList_isDrinkSrv_VIP);

                    #region 対応者(対応部署)

                    List<string> dept = new List<string>() {
                    "Text_DeptInChrg_Rcpt",
                    "Text_DeptInChrg_Sales"
                    };

                    // ラジオボタンセット
                    setYesOrNo(RadioButtonList_DrinkSrv_DeptInChrg_VIP, dept);

                    #endregion 対応者(対応部署)

                    #region 給茶メニュー(SYS_DIVISIONから取得する場合) // 2023.05.15 追加

                    setDropDownListFrmSysDiv(
                        pDivisionId: DIVISION_ID_DRINK_MENU,
                        argDropDownList: DropDownList_DrinkSrv_DrinkMenu_VIP,
                        //insBlankRow: false
                        insBlankRow: true
                        );

                    #endregion

                }

            }

            #endregion お茶出し

            #endregion 固定値ドロップダウン


            #region 表示文字列変更 // 2023.06.06 追加

            #region 見送り // 2023.06.06 追加

            setLiteralText_SndOff(); // 2023.06.06 追加

            #endregion 見送り

            #endregion 表示文字列変更

        }

        /// <summary>
        /// ビルオプションの設定（一部抜粋）
        /// </summary>
        protected void setBuildingOption(Boolean isGuestEntry, int argBuilding_Id)
        {

            DataTable dtBuildingOption = null;
            
            dtBuildingOption = Rsrv_Rgstr.GetBuildingOptionById(argBuilding_Id);

            // 初期表示
            
            //Panel_isVIP.Visible = false; // 「お茶出し」のみ有効の場合に備えて 2023.06.12  コメントアウト
            Panel_isVIP.Visible = Panel_DrinkSrv_VIP.Visible; // 「お茶出し」のみ有効の場合に備えて 2023.06.12 追加
            Panel_PickUp_VIP.Visible = false; // 出迎え(VIPのみ) 新様式 // 2023.05 追加
            //Panel_DrinkSrv_VIP.Visible = false; // お茶出し(VIPのみ) // 2023.05.10 追加 → "setFloorOption()"に移動
            Panel_SendOff_VIP.Visible = false; // 見送り(VIPのみ) // 2023.05.10 追加                        
            
            DropDownList_PickUp_PckUpFrm_VIP.Items.Clear(); // 出迎え(VIPのみ) 新様式 // 2023.05 追加
            //DropDownList_DrinkSrv_DrinkMenu_VIP.Items.Clear(); // お茶出し(VIPのみ) // 2023.05.11 追加 → "setFloorOption()"に移動
            DropDownList_SendOff_SndOffTo_VIP.Items.Clear(); // 見送り(VIPのみ) // 2023.05.10 追加            

            for (int i = 0; i < dtBuildingOption.Rows.Count; i++)
            {               

                #region 出迎え・見送り場所 // 2023.05.09 追加 → 2023.06.12 "国旗掲揚"に移動のためコメントアウト

                //if (true) // 「ビル情報編集画面」に項目追加する際は、この辺りに条件が入ると思われます。
                //{
                //    if ((i == 0) == true) // 「ビル情報編集画面」に項目追加する際は、この辺りに条件が入ると思われます。
                //    {
                #region 会議室予約なら →"国旗掲揚"に移動
                //if (!isGuestEntry)
                //{
                //    Panel_isVIP.Visible = true;

                //    Panel_PickUp_VIP.Visible = true;
                //    Panel_SendOff_VIP.Visible = true;

                //}
                #endregion 会議室予約

                #region 来訪者登録なら // 2023.05.26 追加 →"国旗掲揚"に移動
                //else
                //{
                #region 出迎え

                //// 来訪者登録時に出迎えを表示するかどうか
                //bool isPickUp;
                ////// Web.configから設定をお取得（取得失敗時はtrue「表示する」)）
                ////if (!Boolean.TryParse(ConfigurationManager.AppSettings[Value_GuestEntry_isPickUp], out isPickUp)) 
                //    isPickUp = true;
                //if (isPickUp)
                //{
                //    Panel_isVIP.Visible = true;

                //    Panel_PickUp_VIP.Visible = true;
                //}

                #endregion 出迎え

                #region 見送り

                //// 来訪者登録時に見送りを表示するかどうか
                //bool isSendOff;
                ////// Web.configから設定をお取得（取得失敗時はtrue「表示する」)）
                ////if (!Boolean.TryParse(ConfigurationManager.AppSettings[Value_GuestEntry_isSendOff], out isSendOff)) 
                //    isSendOff = true;
                //if (isSendOff)
                //{
                //    Panel_isVIP.Visible = true;

                //    Panel_SendOff_VIP.Visible = true;
                //}

                #endregion 見送り
                //}
                #endregion 来訪者登録
                //    }
                //}

                //if ((BuildingOption_Value_Name_MeetingPlace.Equals(dtBuildingOption.Rows[i]["VALUE_NAME"])) == true)
                //{
                //    if ((string.IsNullOrEmpty(dtBuildingOption.Rows[i]["VALUE"].ToString())) == false)
                //    {
                //        ListItem item_Meeting = new ListItem();
                //        item_Meeting.Text = dtBuildingOption.Rows[i]["VALUE"].ToString();
                //        item_Meeting.Value = dtBuildingOption.Rows[i]["VALUE_ID"].ToString();
                //        DropDownList_PickUp_PckUpFrm_VIP.Items.Add(item_Meeting);
                //        DropDownList_SendOff_SndOffTo_VIP.Items.Add(item_Meeting);
                //    }
                //}

                // ドロップダウンアイテムを[MST_BUILDING_OPTION]から取得する場合（ビル情報設定画面で編集する様な改修をする場合など）
                //setDropDownListFrmBldgOpt(valueName:BuildingOption_Value_Name_MeetingPlace,
                //    dr: dtBuildingOption.Rows[i],
                //    dropDownLists: new List<DropDownList>() { DropDownList_PickUp_PckUpFrm_VIP, DropDownList_SendOff_SndOffTo_VIP }
                //    );


                #endregion 出迎え・見送り場所

                #region お茶出し // 2023.05.11 追加 → "setFloorOption()"に移動

                //if (true) // setFloorOptionみたいなメソッドを作成して、この辺りの条件詰めるのもありだと思います。
                //{
                //    if ((i == 0) == true) // setFloorOptionみたいなメソッドを作成して、この辺りの条件詰めるのもありだと思います。
                //    {
                #region 会議室予約なら
                //if (!isGuestEntry)
                //{
                //    // 新規予約ならリクエストにFloorIdが入っている前提
                //    var aryFloorId = Page.Request.Form.GetValues(Rsrv_Rgstr.REQUEST_NAME_FloorId);

                //    // 予約済み内容の更新・参照時はHiddenFieldの方が使える前提
                //    if (aryFloorId == null) aryFloorId = new string[] { HiddenField_FloorId_For_Edit.Value };

                //    if (isDrinkSrvFloor(aryFloorId[0]))
                //    {
                //        Panel_isVIP.Visible = true;

                //        Panel_DrinkSrv_VIP.Visible = true;
                //    }
                //    // 判定に使うAreaIdの取得の仕方が上手くないとは思いますので、別の取得方法にしてしまってOKです。

                //}
                #endregion 会議室予約

                #region 来訪者登録なら // 2023.05.24 追加
                //else
                //{
                //    // 来訪者登録時にお茶出しを表示するかどうか
                //    bool isDrinkSrv;
                //    // Web.configから設定をお取得（取得失敗時はfalse「表示しない」)）
                //    if (!Boolean.TryParse(ConfigurationManager.AppSettings[Value_GuestEntry_isDrinkSrv], out isDrinkSrv)) isDrinkSrv = false;
                //    if (isDrinkSrv)
                //    {
                //        Panel_isVIP.Visible = true;

                //        Panel_DrinkSrv_VIP.Visible = true;
                //    }
                //}
                #endregion 来訪者登録

                //    }
                //}

                //// ドロップダウンアイテムを[MST_BUILDING_OPTION]から取得する場合（ビル情報設定画面で編集する様な改修をする場合など）
                ////setDropDownListFrmBldgOpt(valueName: BuildingOption_Value_Name_DrinkMenu,
                ////    dr: dtBuildingOption.Rows[i],
                ////    dropDownLists: new List<DropDownList>() { DropDownList_DrinkSrv_DrinkMenu_VIP }
                ////    );

                #endregion お茶出し

                //#region 国旗掲揚
                //#region 出迎え(VIPのみ) 旧様式
                #region 出迎え(新旧様式)・見送り

                // ビルオプションテーブルのi番目が「国旗掲揚の表示・非表示」なら
                if ((BuildingOption_Value_Name_showCountryFlag.Equals(dtBuildingOption.Rows[i]["VALUE_NAME"])) == true)
                {
                    if (("1".Equals(dtBuildingOption.Rows[i]["VALUE"])) == true)
                    {

                        #region 出迎え(新様式)・見送り // 2023.06.12 追加

                        // 「国旗掲揚」のカラムを流用して、「出迎え」「見送り」の表示・非表示を設定します

                        #region 会議室予約なら
                        if (!isGuestEntry)
                        {
                            Panel_isVIP.Visible = true;

                            Panel_PickUp_VIP.Visible = true;
                            Panel_SendOff_VIP.Visible = true;

                        }
                        #endregion 会議室予約

                        #region 来訪者登録なら
                        else
                        {
                            #region 出迎え

                            // 来訪者登録時に出迎えを表示するかどうか
                            bool isPickUp;
                            //// Web.configから設定をお取得（取得失敗時はtrue「表示する」)）
                            //if (!Boolean.TryParse(ConfigurationManager.AppSettings[Value_GuestEntry_isPickUp], out isPickUp)) 
                            isPickUp = true;
                            if (isPickUp)
                            {
                                Panel_isVIP.Visible = true;

                                Panel_PickUp_VIP.Visible = true;
                            }

                            #endregion 出迎え

                            #region 見送り

                            // 来訪者登録時に見送りを表示するかどうか
                            bool isSendOff;
                            //// Web.configから設定をお取得（取得失敗時はtrue「表示する」)）
                            //if (!Boolean.TryParse(ConfigurationManager.AppSettings[Value_GuestEntry_isSendOff], out isSendOff)) 
                            isSendOff = true;
                            if (isSendOff)
                            {
                                Panel_isVIP.Visible = true;

                                Panel_SendOff_VIP.Visible = true;
                            }

                            #endregion 見送り
                        }
                        #endregion 来訪者登録

                        #endregion 出迎え(新様式)・見送り

                        #region 出迎え(旧国旗掲揚) // 2023.06.12   コメントアウト
                        //Panel_isVIP.Visible = true;                                              

                        //Panel_CountryFlag.Visible = true;
                        #endregion 出迎え(旧国旗掲揚)

                    }
                }
                             
               
                //#endregion 国旗掲揚
                #endregion 出迎え(VIPのみ) 旧様式

                

            #region パネルの表示非表示

            if (Panel_Visitor_Information.Visible == true && (
                Panel_PickUp_VIP.Visible || // 2023.05.16 追加
                Panel_DrinkSrv_VIP.Visible || // 2023.05.16 追加 ("setFloorOption()"との兼ね合いで残している)
                Panel_SendOff_VIP.Visible || // 2023.05.16 追加
                )
            )
            {
                Panel_isVIP.Visible = true;
            }
            else
            {
                Panel_isVIP.Visible = false;
            }

            #endregion パネルの表示非表示

            #region 固定値ドロップダウン

            
            RadioButtonList_isPickUp_VIP.Items.Clear(); // 2023.05.09 追加
            RadioButtonList_PickUp_DeptInChrg_VIP.Items.Clear(); // 2023.05.10 追加

            //RadioButtonList_isDrinkSrv_VIP.Items.Clear(); // 2023.05.10 追加 → "setFloorOption()"に移動
            //RadioButtonList_DrinkSrv_DeptInChrg_VIP.Items.Clear(); // 2023.05.10 追加 → "setFloorOption()"に移動

            RadioButtonList_isSendOff_VIP.Items.Clear(); // 2023.05.10 追加
            RadioButtonList_SendOff_DeptInChrg_VIP.Items.Clear(); // 2023.05.10 追加
            
                      
            //#region 国旗掲揚・駐車場
            #region 出迎え(新旧様式)・お茶出し・見送り・駐車場

            if (Panel_isVIP.Visible)
            {
                ListItem item; 

                item = new ListItem ();
                item.Text = HttpContext.GetGlobalResourceObject("Common", "Text_NoApply").ToString();
                item.Value = "0";
                item.Selected = true;
                RadioButtonList_isEvent.Items.Add(item);

                item = new ListItem();
                item.Text = HttpContext.GetGlobalResourceObject("Common", "Text_Apply").ToString();
                item.Value = "1";
                RadioButtonList_isEvent.Items.Add(item);

                #region 出迎え(新様式) // 2023.05.09 追加
                if (Panel_PickUp_VIP.Visible)
                {
                    // 出迎え要／不要
                    setYesOrNo(RadioButtonList_isPickUp_VIP);

                    #region 対応者(対応部署)

                    #region (没)列挙型を使う場合
                    //List<string> dept = new List<string>();

                    //// 受付
                    //string rcpt = HttpContext.GetGlobalResourceObject("Common", "Text_DeptInChrg_Rcpt").ToString()
                    //    + HttpContext.GetGlobalResourceObject("Common", "Text_DeptInChrg_BizHrs").ToString();
                    //dept.Add(rcpt);

                    //// 営業部署
                    //string sales = HttpContext.GetGlobalResourceObject("Common", "Text_DeptInChrg_Sales").ToString();
                    //dept.Add(sales);
                    #endregion ぼつ

                    List<string> dept = new List<string>() {
                    "Text_DeptInChrg_Rcpt",
                    "Text_DeptInChrg_Sales"
                    };

                    // ラジオボタンセット
                    setYesOrNo(RadioButtonList_PickUp_DeptInChrg_VIP, dept);

                    #endregion 対応者(対応部署)

                    #region 出迎え場所(SYS_DIVISIONから取得する場合) // 2023.05.15 追加

                    setDropDownListFrmSysDiv(
                        pDivisionId: DIVISION_ID_MEETING_PLACE, 
                        argDropDownList: DropDownList_PickUp_PckUpFrm_VIP,
                        //insBlankRow: false
                        insBlankRow: true
                        );

                    #endregion

                }

                #endregion 出迎え(新様式)

                #region お茶出し // 2023.05.10 追加 → "setFloorOption()"に移動
                //if (Panel_DrinkSrv_VIP.Visible)
                //{
                //    // 出迎え要／不要
                //    setYesOrNo(RadioButtonList_isDrinkSrv_VIP);


                    #region 対応者(対応部署)

                    //List<string> dept = new List<string>() {
                    //"Text_DeptInChrg_Rcpt",
                    //"Text_DeptInChrg_Sales"
                    //};

                    //// ラジオボタンセット
                    //setYesOrNo(RadioButtonList_DrinkSrv_DeptInChrg_VIP, dept);

                    #endregion 対応者(対応部署)

                    #region 給茶メニュー(SYS_DIVISIONから取得する場合) // 2023.05.15 追加

                    //setDropDownListFrmSysDiv(
                    //    pDivisionId: DIVISION_ID_DRINK_MENU, 
                    //    argDropDownList: DropDownList_DrinkSrv_DrinkMenu_VIP,
                    //    //insBlankRow: false
                    //    insBlankRow: true
                    //    );

                    #endregion

                //}

                #endregion お茶出し

                #region 見送り // 2023.05.10 追加
                if (Panel_SendOff_VIP.Visible)
                {
                    // 出迎え要／不要
                    setYesOrNo(RadioButtonList_isSendOff_VIP);


                    #region 対応者(対応部署)
                                      
                    List<string> dept = new List<string>() {
                    "Text_DeptInChrg_Rcpt",
                    "Text_DeptInChrg_Sales"
                    };

                    // ラジオボタンセット
                    setYesOrNo(RadioButtonList_SendOff_DeptInChrg_VIP, dept);

                    #endregion 対応者(対応部署)

                    #region 見送り場所(SYS_DIVISIONから取得する場合) // 2023.05.15 追加

                    setDropDownListFrmSysDiv(
                        pDivisionId: DIVISION_ID_MEETING_PLACE,
                        argDropDownList: DropDownList_SendOff_SndOffTo_VIP,
                        //insBlankRow: false
                        insBlankRow: true
                        );

                    #endregion

                }

                #endregion 見送り
                
            }

            //#endregion 国旗掲揚・駐車場
            #endregion 出迎え(旧様式)・駐車場            

            #endregion 固定値ドロップダウン

        }

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

using SmartSharp.TextBuilder;
using System.Diagnostics;

namespace TextBuilder_Tester
{
    public class Test_StrPack
    {
        #region ▼ Initialize

        public string text;
        public string html;
        public string tinyText;
        public string testText;

        private Stopwatch _stopwatch;
        private long _memoriaAntes;

        [SetUp]
        public void Setup()
        {
            html = @"<head>
                                        <meta charset='UTF-8'>
                                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                        <title></title>
                                        <link href='../fonts.css' rel=\'stylesheet'/>
                                        <link href='../styles/vertical/popup_master.css?v=4' rel='stylesheet'/>
                                        <link href='../styles/vertical/popup_visitor.css?v=6' rel='stylesheet'/>
                                        <script src='../scripts/vertical/scriptCheckbutton.js'></script>
                                   </head>
                                   <body style='width:1920px; height:1080px'>
                                   <section id='popup'>
                                        <div id='divPopupVertical' class='divPopupVertical popupColorBack_light popupClosed'>teste
                                            <div id='divUnitPopup' class='divPopupVertical popupColorBack_mid popupOpened' data-unidade=''>                
                                                <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='header'>
                                                    <input type='button' class='btnPopupClose popupColorBack_dark' value='x' onclick='subPopup_vertical_OpenClose()'/>
                                                    <span id='divUnitPopup_block_caption' data-info='label_bloco' class='lblTorre_label popupColorBack_mid'>TORRE:</span>
                                                    <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div>
                                                    <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
                                                    <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
                                                    <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
                                                    <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
                                                </div>
                                            </div>
                                        divPopupVertical</div>
                                        <div class='divPlant popupColorBack_mid'>
                                            <div class='divMetragem'>
                                               <span data-info='metragem' data-form='popup' data-field='metragem' id='lblMetragem_value'>0</span><br>m²
                                            </div>
                                            <div style='height:20px; top:-2px; left:26px;' class='divPlantVerticalLine'></div>
                                            <div style='width:26px; top:40px; left:-2px;' class='divPlantHorizontalLine'></div>
                                            <div style='height:12px; top:40px; left:24px;' class='divPlantVerticalLine'></div>
                                            <div style='height:16px; top:74px; left:24px;' class='divPlantVerticalLine'></div>
                                            <div style='height:8px; top:-2px; left:84px;' class='divPlantVerticalLine'></div>
                                            <div id='divValorFinal' style='width:32px; top:44px; left:84px;' class='divPlantHorizontalLine'></div>
                                        </div>
                                    </section>";

            text = @"PRIVATE INSTRUMENT OF PROMISE OF PURCHASE AND SALE OF PROPERTY
                      
                      SUBJECT OF SUBDIVISION
                      
                      Summary Table

                      A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private law company, duly registered 
                           with CNPJ under number 74.099.252/0001-56, headquartered at Avenida José Ferreira 
                           Batista react, nº2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance with its Articles of incorporation, in the capacity 
                           of Owner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Silva, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.

                      B. THE SUBDIVISION:

                      The type of property is a residential subdivision, named LOTEAMENTO RESIDENCIAL BARCELONA, located 
                      in Araçatuba, State of São Paulo, registered under property record nº 100.314 at the Real Estate 
                      Registry Office of the District of Araçatuba – State of São Paulo.

                      B.1. Subdivision Description:
                           The LOTEAMENTO RESIDENCIAL BARCELONA will be developed according to the specifications 
                           contained in the Descriptive Report, copies of which will be archived along with the 
                           registration process of the development.
                           The BUYER is fully and unequivocally aware that the information contained in the Conceptual 
                           Plan is merely illustrative and may be modified according to the needs of the project and 
                           the interest of the SELLER and/or determinations from the competent public authorities.

                      B.2. Basic Characteristics of the Subdivision:

                      - Name: Jardim Barcelona, cidade de Araraquara/SP.
                      - Total Area: 315,467.00 m².
                      - Area allocated for Residential Lots: 136,705.37 m².
                      - Area allocated for Commercial Lots: 3,031.19 m².
                      - Leisure System: 27,381.80 m².
                      - Area allocated for the Entrance Gate: 704.85 m².
                      - Green Areas: 40,860.29 m².
                      - Total Road System: 91,579.16 m².
                      - Institutional Areas: 15,909.19 m².
                      - Total Number of Lots: 609.
                      - Residential Lots: 608.
                      - Commercial Lots: 01.

                      B.3. Deadline for completion of infrastructure works:
                           24 (twenty-four) months, counted from the date of the public launch of the subdivision, 
                           which was carried out on December 5, 2015, and may be anticipated at any time or extended 
                           for a period granted by the Municipality of Araçatuba-SP, under the terms of this instrument.";

            tinyText = @"A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.";

            testText = @"A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, 
                           married, registered under action and react law company, duly act registered 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.";

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Medição de memória antes
            _memoriaAntes = GC.GetTotalMemory(true);

            // Inicia o cronômetro
            _stopwatch = Stopwatch.StartNew();
        }

        [TearDown]
        public void TearDown()
        {
            _stopwatch.Stop();

            long memoriaDepois = GC.GetTotalMemory(true);
            long memoriaConsumida = memoriaDepois - _memoriaAntes;

            //_stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "o método deve ser rápido")
            //memoriaConsumida.Should().BeLessThan(1024 * 1024, "o método não deve consumir muita memória"); // 1MB

            Console.WriteLine($"Tempo de execução: {_stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Memória consumida: {memoriaConsumida} bytes");
        }

        #endregion

        #region ▼ Words

        #region ► Litteral Match

        [Test]
        public void Test01_Match()
        {
           StringAndPosition firstMatch = TextBuilder.Match(text,"Marie Doe||Jane Doe||Jack||John Doe");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else
            { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN: John Doe - 842
        }

        [Test]
        public void Test01a_Match_IgnoreSingleQuotes()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "John Doe||Marie Doe", TextBuilder.ParamsIgnoreInQuotes);

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else
            { Console.WriteLine(firstMatch.Text); }

            //Duration: 3ms, Memory: 352 bytes

            //RETURN: Marie Doe - 861
        }

        [Test]
        public void Test01b_Match_IgnoreCase()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "john doe||marie doe", TextBuilder.ParamsIgnoreCaseSensitive);

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else
            { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN: John Doe
        }

        #endregion

        #region ► Pattern Match

        [Test]
        public void Test02_MatchPattern_PatternInStart()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, @"*residential");
            Console.WriteLine(firstMatch.Text);

            //Duration: 2ms, Memory: 7296 bytes


            //RETURN:
            /*
             residential subdivision, named LOTEAMENTO RESIDENCIAL BARCELONA, located 
                      in Araçatuba, State of São Paulo, registered under property record nº 100.314 at the Real Estate 
                      Registry Office of the District of Araçatuba – State of São Paulo.

                      B.1. Subdivision Description:
                           The LOTEAMENTO RESIDENCIAL BARCELONA will be developed according to the specifications 
                           contained in the Descriptive Report, copies of which will be archived along with the 
                           registration process of the development.
                           The BUYER is fully and unequivocally aware that the information contained in the Conceptual 
                           Plan is merely illustrative and may be modified according to the needs of the project and 
                           the interest of the SELLER and/or determinations from the competent public authorities.

                      B.2. Basic Characteristics of the Subdivision:

                      - Name: Jardim Barcelona, cidade de Araraquara/SP.
                      - Total Area: 315,467.00 m².
                      - Area allocated for Residential Lots: 136,705.37 m².
                      - Area allocated for Commercial Lots: 3,031.19 m².
                      - Leisure System: 27,381.80 m².
                      - Area allocated for the Entrance Gate: 704.85 m².
                      - Green Areas: 40,860.29 m².
                      - Total Road System: 91,579.16 m².
                      - Institutional Areas: 15,909.19 m².
                      - Total Number of Lots: 609. - 1698
             */
        }

        [Test]
        public void Test02a_MatchPattern_PatternInMiddle()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "J*ner");

            if (firstMatch.Empty) { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 456 bytes

            //RETURN:
            //John Doe Towner*/

        }
                
        [Test]
        public void Test02b_MatchPattern_PatternInEnd()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "B.3.*");

            if (firstMatch.Empty)
            {
                Console.WriteLine("Ocorrencia não encontrada!");
            }
            else
            {
                Console.WriteLine(firstMatch.Text);
            }

            //Duration: 3ms, Memory: 2112 bytes

            //RETURN:
            /*B.3. Deadline for completion of infrastructure works:
                           24 (twenty-four) months, counted from the date of the public launch of the subdivision, 
                           which was carried out on December 5, 2015, and may be anticipated at any time or extended 
                           for a period granted by the Municipality of Araçatuba-SP, under the terms of this instrument.*/

        }

        [Test]
        public void Test02c_MatchPattern_PatternInMiddleAndEnd()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "Name*Jard*.");
            Console.WriteLine(firstMatch.Text);

            //Duration: 2ms, Memory: 656 bytes

            //RETURN:
            // Name: Jardim Barcelona, cidade de Araraquara/SP. - 2792 */

        }

        [Test]
        public void Test02d_MatchPattern_MoreThanOnePattern()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "Name:*cidade de *.");

            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 656 bytes

            //RETURN:
            // Name: Jardim Barcelona, cidade de Araraquara/SP. */

        }

        [Test]
        public void Test02e_MatchPattern_PatternWithOrCondition_wrong_OR_use()
        {
            /*Wrong 'OR" condition use. Did put '@' after wildcard and before 'OR' char condition. 
             * So the '@' is alone and it is a more one option in condition and not a char linked 
             * of email servers of the 'OR' condition.*/

            StringAndPosition firstMatch = TextBuilder.Match(text, @"email*@|hotmail.com|gmail.com|yahoo.com");
            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:
            /*email marie@*/
        }

        //[Test]
        //public void Test07a_MatchPattern_PatternWithOrCondition()
        //{
        //    /* Another wrong 'OR' use.
        //     * Not use it to start, end pattern and after wildcard '*'
        //     */
        //    StringAndPosition firstMatch = TextBuilder.Match(text, @"email*|@hotmail.com|@gmail.com|@yahoo.com");
        //    if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
        //    else { Console.WriteLine(firstMatch.Text); }

        //    //Duration: 2ms, Memory: 456 bytes

        //    //RETURN:
        //    /*Invalid use of 'OR' char('|')! Not use it to start, end pattern and after wildcard '*'*/
        //}

        [Test]
        public void Test02f_MatchPattern_PatternWithOrCondition()
        {
            /*This test is using right the 'OR' condition.*/
            StringAndPosition firstMatch = TextBuilder.Match(text, @"email*@hotmail.com|@gmail.com|@yahoo.com");
            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 456 bytes

            //RETURN:
            /*email marie@gmail.com*/
        }

        [Test]
        public void Test02g_MatchPattern_PatternWithOrCondition()
        {
            /*Is sure exist a more better form of do This pattern*/
            StringAndPosition firstMatch = TextBuilder.Match(text, @"email*.com|.com.br");
            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 456 bytes

            //RETURN:
            /*email marie@gmail.com*/
        }

        [Test]
        public void Test02h_MatchPattern_PatternWithOrCondition()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, @"married*Marie|John|Jack* Mcan| Albert| Towner");
            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 456 bytes

            //RETURN:
            /*married with 'John Doe Towner*/
        }

        [Test]
        public void Test02i_MatchPattern_PatternWithOrConditionIgnoreCase()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, @"married*marie|john|jack", TextBuilder.ParamsIgnoreCaseSensitive);
            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 3ms, Memory: 456 bytes

            //RETURN:
            /*married with 'John*/
        }

        #endregion

        #region ► Dynamic Match

        [Test]
        public void Test03_Match_SeparatorWord_without_separator()
        {
            StringAndPosition firstMatch = TextBuilder.Match(tinyText, "act");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine("[" + firstMatch.Text + "]"); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN: [act]
            //Result is a piece of "reactor" word in tinyText.
            //Phrase in text: " A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private re[act]ion law company"
        }

        [Test]
        public void Test03a_Match_SeparatorWord_in_start()
        {
            StringAndPosition firstMatch = TextBuilder.Match(testText, "_act", TextBuilder.ParamsDynamicChars);
            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine("[" + firstMatch.Text + "]"); }

            //Duration: 2ms, Memory: 384 bytes

            //RETURN: [ act] 
            /*Return occurrence is the apart of "react" word in tinyText. 
            Phrase in text: "Batista re[act], nº 2281, action room 02, Bairro Ipanema, in this City and District 
            of Araçatuba-SP"*/

        }

        [Test]
        public void Test03b_Match_SeparatorWord_in_start_CutStartChar()
        {
            StringAndPosition firstMatch = TextBuilder.Match( testText, 
                                                 "_act", 
                                                 0,
                                                 1,
                                                 TextBuilder.ParamsDynamicChars);
            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine("[" + firstMatch.Text + "] - " + firstMatch.Position); }

            //Duration: 2ms, Memory: 384 bytes

            //RETURN: [act] - 161
            /*The same of previous exemple, but since user just want return the word, it used a 'startIndexReturn' paramater and with it can remove a 
             frist char of occurence return that this case is a space.*/
        }

        [Test]
        public void Test03c_Match_SeparatorWord_separator_in_end()
        {
            StringAndPosition firstMatch = TextBuilder.Match(testText, "act_", TextBuilder.ParamsDynamicChars);

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine("[" + firstMatch.Text + "] - " + firstMatch.Position); }

            //Duration: 2ms, Memory: 384 bytes

            //RETURN: [act ] - 174
            /*Return occurrence is a part of "action" word in tinyText. 
             *Phrase in text: "Avenida José Ferreira Batista react, nº 2281, [act]ion room 02"*/
        }

        [Test]
        public void Test03d_Match_SeparatorWord_separator_start_end()
        {
            StringAndPosition firstMatch = TextBuilder.Match(testText, "_act_", TextBuilder.ParamsDynamicChars);

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine("[" + firstMatch.Text + "] - " + firstMatch.Position); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:[ act ] - 195
            //Return is exactly word "act" in tinyText.
            //Phrase in text: "represented in this [act] in accordance and notion with its Articles"
        }

        [Test]
        public void Test03d_Match_SeparatorWord_separator_start_end_CutStartEndChar()
        {
            StringAndPosition firstMatch = TextBuilder.Match(testText, "_act_",0, 1, 1, TextBuilder.ParamsDynamicChars);

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine("[" + firstMatch.Text + "] - " + firstMatch.Position); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN: [act] - 196
            //Same of previous exemple, but used 'startIndexReturn' and 'endCutLenReturn' to remove start and end
            //separator of occurence return"
        }

        [Test]
        public void Test04_MatchDynamic_startCompleteWord()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "~on", TextBuilder.ParamsDynamicChars);

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:
            /*action*/
        }

        [Test]
        public void Test04a_MatchDynamic_EndCompleteWord()
        {
            StringAndPosition firstMatch = TextBuilder.Match(testText, "act~", TextBuilder.ParamsDynamicChars);

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 3ms, Memory: 352 bytes

            //RETURN:
            /*action*/
        }

        [Test]
        public void Test04b_MatchDynamic_startCompleteWord_ignoreCase()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "~on", TextBuilder.ParamsDynamicChars, TextBuilder.ParamsIgnoreCaseSensitive);

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:
            /*SUBDIVISION*/
        }

        [Test]
        public void Test04c_MatchDynamic_StartAndEndCompleteWord()
        {
            StringAndPosition firstMatch = TextBuilder.Match(testText, "~ti~", TextBuilder.ParamsDynamicChars);

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position); }

            //Duration: 3ms, Memory: 352 bytes

            //RETURN:
            /*national - 88*/
        }

        [Test]
        public void Test05_MatchDynamic_NumbersChar()
        {
            StringAndPosition firstMatch = TextBuilder.Match(testText, "#.#.#-#", TextBuilder.ParamsDynamicChars);

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:
            /*675.019.610-18*/
        }

        [Test]
        public void Test05a_MatchDynamic_StartNumbersChar()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "nº#", TextBuilder.ParamsDynamicChars);

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:
            /*nº2281*/
        }

        [Test]
        public void Test05b_MatchDynamic_EndNumbersChar()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "B.#.", TextBuilder.ParamsDynamicChars);

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:
            /*B.1.*/
        }

        [Test]
        public void Test06_MatchDynamic_DynmaicsAndWildcard()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "Road System: #*m²", TextBuilder.ParamsDynamicChars);

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position); }

            //Duration: 3ms, Memory: 592 bytes

            //RETURN:
            /*Road System: 91,579.16 m² - 3264*/
        }

        #endregion

        #region ▼ Repalce

        [Test]
        public void Test08_ReplaceFirst()
        {
            string replaceMatch = TextBuilder.ReplaceFirst(tinyText, "Joh*Towner", "Bruce Banner");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6152 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'Bruce Banner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test08a_ReplaceLast()
        {
            string replaceMatch = TextBuilder.ReplaceLast(tinyText, "Joh*Towner", "Bruce Banner");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6152 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to Bruce Banner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test08b_Replace()
        {
            string replaceMatch = TextBuilder.Replace(tinyText, "D*Towner", "Wayne Silva");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6184 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to Bruce Banner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        #endregion

        #region ▼ Insert

        [Test]
        public void Test09_Insert()
        {
            string replaceMatch = TextBuilder.Insert(tinyText, " the client", 686);
            Console.WriteLine(replaceMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner the client' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test09a_InsertBeforeFirst()
        {
            string replaceMatch = TextBuilder.InsertBeforeFirst(tinyText, " the respected sr ", "John Doe");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6224 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with ' the respected sr John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test09b_InsertBefore()
        {
            string replaceMatch = TextBuilder.InsertBefore(tinyText, " the respected sr ", "John Doe");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6224 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with ' the respected sr John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }
        
        [Test]
        public void Test09d_InsertAfterFirst()
        {
            string replaceMatch = TextBuilder.InsertAfterFirst(tinyText, " the mr client", "Joh*Doe");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6232 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe the ms client  Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test09e_InsertAfter()
        {
            string replaceMatch = TextBuilder.InsertAfter(tinyText, " the mr client", "Joh*Doe");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6232 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe the ms client  Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        #endregion

        #region ▼ Remove

        [Test]
        public void Test10_RemoveFirst()
        {
            string replaceMatch = TextBuilder.RemoveFirst(tinyText, "John ");
            Console.WriteLine(replaceMatch);

            //Duration: 1ms, Memory: 6152 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test10a_RemoveLast()
        {
            string replaceMatch = TextBuilder.RemoveLast(tinyText, "John ");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6152 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test10b_Remove()
        {
            string replaceMatch = TextBuilder.Remove(tinyText, "John ");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6152 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        #endregion

        #region ▼ Contains

        [Test]
        public void Test11_Contains()
        {
            bool returnContains = TextBuilder.Contains(tinyText, "John ");
            Console.WriteLine(returnContains);

            //Duration: 3ms, Memory: 352 bytes

            /*RETURN: 
             
             */
        }

        [Test]
        public void Test11a_Contains()
        {
            bool returnContains = TextBuilder.Contains(tinyText, "kkkkkkkbua");
            Console.WriteLine(returnContains);

            //Duration: 3ms, Memory: 352 bytes

            /*RETURN: 
             
             */
        }

        [Test]
        public void Test11b_Contains()
        {
            bool returnContains = TextBuilder.Contains(tinyText, "Mar*ner");
            Console.WriteLine(returnContains);

            //Duration: 3ms, Memory: 352 bytes

            /*RETURN: 
             
             */
        }

        #endregion

        #region ▼ ToLower

        [Test]
        public void Test12_ToLower()
        {
            string returnToLower = TextBuilder.ToLowerChar(tinyText, 'J');
            Console.WriteLine(returnToLower);

            //Duration: 2ms, Memory: 6144 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPj under number 22.724.722/0001-21, headquartered at Avenida josé Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'john Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to john Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test12a_ToLowerMatch()
        {
            string returnToLower = TextBuilder.ToLowerMatch(tinyText, "LO*CIAL");
            Console.WriteLine(returnToLower);

            //Duration: 3ms, Memory: 6144 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. loteamento residencial BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test12b_ToLowerIgnoreSnippet()
        {
            string returnToLower = TextBuilder.ToLowerIgnoreInSnippet(tinyText, ("A.1.", "SELLER"));
            Console.WriteLine(returnToLower);

            //Duration: 1ms, Memory: 6144 bytes

            /*RETURN: 
             a. parties
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      a.2. promising buyer(s), married with 'john doe towner' , marie doe towner is brazilian national, broker, married, registered under 
                           cpf number 675.019.610-18, rg number 23.300.225-3 ssp, residing at rua xv de novembro, 3456, 
                           apt. 21 c, centro district, postal code 04021-002, located in the city of são paulo/sp, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to john doe towner, registered under cpf number 012.869.980-93, 
                           rg number 102.456.543-2 ssp, in partial community property regime, brazilian national, lawyer, 
                           hereinafter simply referred to as buyer.
             */
        }

        [Test]
        public void Test12c_ToLowerOnlyInSnippet()
        {
            string returnToLower = TextBuilder.ToLowerOnlyInSnippet(tinyText, ("A.1.", "SELLER"));
            Console.WriteLine(returnToLower);

            //Duration: 1ms, Memory: 6144 bytes

            /*RETURN: 
             a.1. loteamento residencial barcelona ltda, a private reaction law company, duly reactor registered 
                           with cnpj under number 22.724.722/0001-21, headquartered at avenida josé ferreira 
                           batista react, nº 2281, action room 02, bairro ipanema, in this city and district of araçatuba-sp, 
                           represented in this act in accordance and notion with its articles of incorporation, in the capacity 
                           of louis doe towner and developer, hereinafter simply referred to as seller.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        #endregion

        #region ▼ ToUpper

        [Test]
        public void Test13_ToUpper()
        {
            string returnToLower = TextBuilder.ToUpperChar(tinyText, 'o');
            Console.WriteLine(returnToLower);

            //Duration: 1ms, Memory: 6144 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPj under number 22.724.722/0001-21, headquartered at Avenida josé Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'john Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to john Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test13a_ToUpperMatch()
        {
            string returnToLower = TextBuilder.ToUpperMatch(tinyText, "Jo*Towner");
            Console.WriteLine(returnToLower);

            //Duration: 3ms, Memory: 6144 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. loteamento residencial BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test13b_ToUpperIgnoreSnippet()
        {
            string returnToLower = TextBuilder.ToUpperIgnoreInSnippet(tinyText, ("A.1.", "SELLER"));
            Console.WriteLine(returnToLower);

            //Duration: 1ms, Memory: 6144 bytes

            /*RETURN: 
             a. parties
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      a.2. promising buyer(s), married with 'john doe towner' , marie doe towner is brazilian national, broker, married, registered under 
                           cpf number 675.019.610-18, rg number 23.300.225-3 ssp, residing at rua xv de novembro, 3456, 
                           apt. 21 c, centro district, postal code 04021-002, located in the city of são paulo/sp, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to john doe towner, registered under cpf number 012.869.980-93, 
                           rg number 102.456.543-2 ssp, in partial community property regime, brazilian national, lawyer, 
                           hereinafter simply referred to as buyer.
             */
        }

        [Test]
        public void Test13c_ToUpperOnlyInSnippet()
        {
            string returnToLower = TextBuilder.ToUpperOnlyInSnippet(tinyText, ("A.1.", "SELLER"));
            Console.WriteLine(returnToLower);

            //Duration: 1ms, Memory: 6144 bytes

            /*RETURN: 
             A.1. loteamento residencial barcelona ltda, a private reaction law company, duly reactor registered 
                           with cnpj under number 22.724.722/0001-21, headquartered at avenida josé ferreira 
                           batista react, nº 2281, action room 02, bairro ipanema, in this city and district of araçatuba-sp, 
                           represented in this act in accordance and notion with its articles of incorporation, in the capacity 
                           of louis doe towner and developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        #endregion

        #region ▼ Translate

        [Test]
        public void Test13d_Translate()
        {
            string returnTranslate = TextBuilder.Translate(tinyText, "2,'a','o','J'", "^,#,&,@");
            Console.WriteLine(returnTranslate);

            //Duration: 1ms, Memory: 6144 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, # priv#te re#cti&n l#w c&mp#ny, duly re#ct&r registered 
                           with CNP@ under number ^^.7^4.7^^/0001-^1, he#dqu#rtered #t Avenid# @&sé Ferreir# 
                           B#tist# re#ct, nº ^^81, #cti&n r&&m 0^, B#irr& Ip#nem#, in this City #nd District &f Ar#ç#tub#-SP, 
                           represented in this #ct in #cc&rd#nce #nd n&ti&n with its Articles &f Inc&rp&r#ti&n, in the c#p#city 
                           &f L&uis D&e T&wner #nd Devel&per, herein#fter simply referred t& #s SELLER.
                      A.^. PROMISING BUYER(S), m#rried with '@&hn D&e T&wner' , M#rie D&e T&wner is Br#zili#n n#ti&n#l, br&ker, m#rried, registered under 
                           CPF number 675.019.610-18, RG number ^3.300.^^5-3 SSP, residing #t Ru# XV de N&vembr&, 3456, 
                           Apt. ^1 C, Centr& district, p&st#l c&de 040^1-00^, l&c#ted in the city &f Sã& P#ul&/SP, 
                           pr&viding c&nt#ct inf&rm#ti&n: ph&ne (11) 34134-00^1, m&bile (11) 98134-00^1, #nd 
                           em#il m#rie@gm#il.c&m; m#rried t& @&hn D&e T&wner, registered under CPF number 01^.869.980-93, 
                           RG number 10^.456.543-^ SSP, in p#rti#l c&mmunity pr&perty regime, Br#zili#n n#ti&n#l, l#wyer, 
                           herein#fter simply referred t& #s BUYER.
             */
        }

        #endregion

        #endregion

        #region ▼ Snippets

        #region ▼ Matches

        [Test]
        public void Test14_Snippet()
        {
            StringAndPosition snippetMatch = TextBuilder.Snippet(html, @"<div*</div>");
            
            if(snippetMatch.Empty)
            {  Console.WriteLine("Snippet not found!"); }
            else {  Console.WriteLine(snippetMatch.Text); }

            //Duration: 3ms, Memory: 7240 bytes

            /*RETURN:
             <div id='divPopupVertical' class='divPopupVertical popupColorBack_light popupClosed'>teste
                                            <div id='divUnitPopup' class='divPopupVertical popupColorBack_mid popupOpened' data-unidade=''>                
                                                <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='header'>
                                                    <input type='button' class='btnPopupClose popupColorBack_dark' value='x' onclick='subPopup_vertical_OpenClose()'/>
                                                    <span id='divUnitPopup_block_caption' data-info='label_bloco' class='lblTorre_label popupColorBack_mid'>TORRE:</span>
                                                    <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'></div>
                                                    <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
                                                    <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
                                                    <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
                                                    <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
                                                </div>
                                            </div>
                                        divPopupVertical</div>
             */
        }

        [Test]
        public void Test14a_SnippetID()
        {
            StringAndPosition snippetMatch = TextBuilder.Snippet(html, "<div *</div>", "id='divTemp'");
            
            if (snippetMatch.Empty)
            { Console.WriteLine("Snippet not found!"); }
            else { Console.WriteLine(snippetMatch.Text); }

            //Duration: 2ms, Memory: 5568 bytes
        }

        [Test]
        public void Test14b_Snippet_IgnoreApotrophesContent()
        {
            StringAndPosition snippetMatch = TextBuilder.Snippet(html, "<div *</div>", "divTemp", TextBuilder.ParamsIgnoreInQuotes);

            if (snippetMatch.Empty)
            { Console.WriteLine("Snippet not found!"); }
            else { Console.WriteLine(snippetMatch.Text); }

            //Duration: 3ms, Memory: 880 bytes

            /*RETURN:
             * <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div> */
        }

        [Test]
        public void Test14c_Snippet_IgnoreCase()
        {
            StringAndPosition snippetMatch = TextBuilder.Snippet(html, "<div *</div>", "'divtemp'", TextBuilder.ParamsIgnoreCaseSensitive);

            if (snippetMatch.Empty)
            { Console.WriteLine("Snippet not found!"); }
            else { Console.WriteLine(snippetMatch.Text); }

            //Duration: 3ms, Memory: 5568 bytes

            /*RETURN:
             * <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div>
                                                    <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
                                                    <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
                                                    <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
                                                    <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
                                                </div>
             */
        }

        #endregion

        #region ▼ Repalce

        [Test]
        public void Test15_ReplaceFirst()
        {
            string replaceMatch = TextBuilder.ReplaceFirst(tinyText, "Joh*Towner", "Bruce Banner");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6152 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'Bruce Banner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test15a_ReplaceLast()
        {
            string replaceMatch = TextBuilder.ReplaceLast(tinyText, "Joh*Towner", "Bruce Banner");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6152 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to Bruce Banner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test15b_Replace()
        {
            string replaceMatch = TextBuilder.Replace(tinyText, "D*Towner", "Wayne Silva");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6184 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to Bruce Banner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        #endregion

        #region ▼ Insert

        [Test]
        public void Test16_Insert()
        {
            string replaceMatch = TextBuilder.Insert(tinyText, " the client ", 686);
            Console.WriteLine(replaceMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner the client ' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test16a_InsertBefore()
        {
            string replaceMatch = TextBuilder.InsertBefore(tinyText, " the respected sr ", "John Doe");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6224 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with ' the respected sr John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test16b_InsertAfter()
        {
            string replaceMatch = TextBuilder.InsertAfter(tinyText, " the ms client ", "Marie Doe");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6232 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe the ms client  Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        #endregion

        #region ▼ Remove

        [Test]
        public void Test17_RemoveFirst()
        {
            string replaceMatch = TextBuilder.RemoveFirst(tinyText, "John ");
            Console.WriteLine(replaceMatch);

            //Duration: 1ms, Memory: 6152 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test17a_RemoveLast()
        {
            string replaceMatch = TextBuilder.RemoveLast(tinyText, "John ");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6152 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        [Test]
        public void Test17b_Remove()
        {
            string replaceMatch = TextBuilder.Remove(tinyText, "John ");
            Console.WriteLine(replaceMatch);

            //Duration: 3ms, Memory: 6152 bytes

            /*RETURN: 
             A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº 2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.
             */
        }

        #endregion

        #region ▼ Contains

        [Test]
        public void Test18_Contains()
        {
            bool returnContains = TextBuilder.Contains(tinyText, "John ");
            Console.WriteLine(returnContains);

            //Duration: 3ms, Memory: 352 bytes

            /*RETURN: 
             
             */
        }

        #endregion

        #endregion
    }
}

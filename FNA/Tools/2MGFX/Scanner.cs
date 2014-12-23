// Generated by TinyPG v1.3 available at www.codeproject.com

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace TwoMGFX
{
    #region Scanner

    public partial class Scanner
    {
        public string Input;
        public int StartPos = 0;
        public int EndPos = 0;
        public string CurrentFile;
        public int CurrentLine;
        public int CurrentColumn;
        public int CurrentPosition;
        public List<Token> Skipped; // tokens that were skipped
        public Dictionary<TokenType, Regex> Patterns;

        private Token LookAheadToken;
        private List<TokenType> Tokens;
        private List<TokenType> SkipList; // tokens to be skipped
        private readonly TokenType FileAndLine;

        public Scanner()
        {
            Regex regex;
            Patterns = new Dictionary<TokenType, Regex>();
            Tokens = new List<TokenType>();
            LookAheadToken = null;
            Skipped = new List<Token>();

            SkipList = new List<TokenType>();
            SkipList.Add(TokenType.BlockComment);
            SkipList.Add(TokenType.Comment);
            SkipList.Add(TokenType.Whitespace);
            SkipList.Add(TokenType.LinePragma);
            FileAndLine = TokenType.LinePragma;

            regex = new Regex(@"/\*([^*]|\*[^/])*\*/", RegexOptions.Compiled);
            Patterns.Add(TokenType.BlockComment, regex);
            Tokens.Add(TokenType.BlockComment);

            regex = new Regex(@"//[^\n\r]*", RegexOptions.Compiled);
            Patterns.Add(TokenType.Comment, regex);
            Tokens.Add(TokenType.Comment);

            regex = new Regex(@"[ \t\n\r]+", RegexOptions.Compiled);
            Patterns.Add(TokenType.Whitespace, regex);
            Tokens.Add(TokenType.Whitespace);

            regex = new Regex(@"^[ \t]*#line[ \t]*(?<Line>\d*)[ \t]*(\""(?<File>[^\""\\]*(?:\\.[^\""\\]*)*)\"")?\n", RegexOptions.Compiled);
            Patterns.Add(TokenType.LinePragma, regex);
            Tokens.Add(TokenType.LinePragma);

            regex = new Regex(@"pass", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Pass, regex);
            Tokens.Add(TokenType.Pass);

            regex = new Regex(@"technique", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Technique, regex);
            Tokens.Add(TokenType.Technique);

            regex = new Regex(@"sampler1D|sampler2D|sampler3D|samplerCUBE|SamplerState|sampler", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Sampler, regex);
            Tokens.Add(TokenType.Sampler);

            regex = new Regex(@"sampler_state", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.SamplerState, regex);
            Tokens.Add(TokenType.SamplerState);

            regex = new Regex(@"VertexShader", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.VertexShader, regex);
            Tokens.Add(TokenType.VertexShader);

            regex = new Regex(@"PixelShader", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.PixelShader, regex);
            Tokens.Add(TokenType.PixelShader);

            regex = new Regex(@"register", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Register, regex);
            Tokens.Add(TokenType.Register);

            regex = new Regex(@"true|false|0|1", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Boolean, regex);
            Tokens.Add(TokenType.Boolean);

            regex = new Regex(@"[+-]? ?[0-9]?\.?[0-9]+[fF]?", RegexOptions.Compiled);
            Patterns.Add(TokenType.Number, regex);
            Tokens.Add(TokenType.Number);

            regex = new Regex(@"[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);
            Patterns.Add(TokenType.Identifier, regex);
            Tokens.Add(TokenType.Identifier);

            regex = new Regex(@"{", RegexOptions.Compiled);
            Patterns.Add(TokenType.OpenBracket, regex);
            Tokens.Add(TokenType.OpenBracket);

            regex = new Regex(@"}", RegexOptions.Compiled);
            Patterns.Add(TokenType.CloseBracket, regex);
            Tokens.Add(TokenType.CloseBracket);

            regex = new Regex(@"=", RegexOptions.Compiled);
            Patterns.Add(TokenType.Equals, regex);
            Tokens.Add(TokenType.Equals);

            regex = new Regex(@":", RegexOptions.Compiled);
            Patterns.Add(TokenType.Colon, regex);
            Tokens.Add(TokenType.Colon);

            regex = new Regex(@",", RegexOptions.Compiled);
            Patterns.Add(TokenType.Comma, regex);
            Tokens.Add(TokenType.Comma);

            regex = new Regex(@";", RegexOptions.Compiled);
            Patterns.Add(TokenType.Semicolon, regex);
            Tokens.Add(TokenType.Semicolon);

            regex = new Regex(@"\|", RegexOptions.Compiled);
            Patterns.Add(TokenType.Or, regex);
            Tokens.Add(TokenType.Or);

            regex = new Regex(@"\(", RegexOptions.Compiled);
            Patterns.Add(TokenType.OpenParenthesis, regex);
            Tokens.Add(TokenType.OpenParenthesis);

            regex = new Regex(@"\)", RegexOptions.Compiled);
            Patterns.Add(TokenType.CloseParenthesis, regex);
            Tokens.Add(TokenType.CloseParenthesis);

            regex = new Regex(@"\[", RegexOptions.Compiled);
            Patterns.Add(TokenType.OpenSquareBracket, regex);
            Tokens.Add(TokenType.OpenSquareBracket);

            regex = new Regex(@"\]", RegexOptions.Compiled);
            Patterns.Add(TokenType.CloseSquareBracket, regex);
            Tokens.Add(TokenType.CloseSquareBracket);

            regex = new Regex(@"<", RegexOptions.Compiled);
            Patterns.Add(TokenType.LessThan, regex);
            Tokens.Add(TokenType.LessThan);

            regex = new Regex(@">", RegexOptions.Compiled);
            Patterns.Add(TokenType.GreaterThan, regex);
            Tokens.Add(TokenType.GreaterThan);

            regex = new Regex(@"compile", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Compile, regex);
            Tokens.Add(TokenType.Compile);

            regex = new Regex(@"[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);
            Patterns.Add(TokenType.ShaderModel, regex);
            Tokens.Add(TokenType.ShaderModel);

            regex = new Regex(@"[\S]+", RegexOptions.Compiled);
            Patterns.Add(TokenType.Code, regex);
            Tokens.Add(TokenType.Code);

            regex = new Regex(@"^$", RegexOptions.Compiled);
            Patterns.Add(TokenType.EndOfFile, regex);
            Tokens.Add(TokenType.EndOfFile);

            regex = new Regex(@"MinFilter", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MinFilter, regex);
            Tokens.Add(TokenType.MinFilter);

            regex = new Regex(@"MagFilter", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MagFilter, regex);
            Tokens.Add(TokenType.MagFilter);

            regex = new Regex(@"MipFilter", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MipFilter, regex);
            Tokens.Add(TokenType.MipFilter);

            regex = new Regex(@"Filter", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Filter, regex);
            Tokens.Add(TokenType.Filter);

            regex = new Regex(@"Texture", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Texture, regex);
            Tokens.Add(TokenType.Texture);

            regex = new Regex(@"AddressU", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.AddressU, regex);
            Tokens.Add(TokenType.AddressU);

            regex = new Regex(@"AddressV", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.AddressV, regex);
            Tokens.Add(TokenType.AddressV);

            regex = new Regex(@"AddressW", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.AddressW, regex);
            Tokens.Add(TokenType.AddressW);

            regex = new Regex(@"MaxAnisotropy", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MaxAnisotropy, regex);
            Tokens.Add(TokenType.MaxAnisotropy);

            regex = new Regex(@"MaxMipLevel|MaxLod", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MaxMipLevel, regex);
            Tokens.Add(TokenType.MaxMipLevel);

            regex = new Regex(@"MipLodBias", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MipLodBias, regex);
            Tokens.Add(TokenType.MipLodBias);

            regex = new Regex(@"Clamp", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Clamp, regex);
            Tokens.Add(TokenType.Clamp);

            regex = new Regex(@"Wrap", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Wrap, regex);
            Tokens.Add(TokenType.Wrap);

            regex = new Regex(@"Mirror", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Mirror, regex);
            Tokens.Add(TokenType.Mirror);

            regex = new Regex(@"None", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.None, regex);
            Tokens.Add(TokenType.None);

            regex = new Regex(@"Linear", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Linear, regex);
            Tokens.Add(TokenType.Linear);

            regex = new Regex(@"Point", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Point, regex);
            Tokens.Add(TokenType.Point);

            regex = new Regex(@"Anisotropic", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Anisotropic, regex);
            Tokens.Add(TokenType.Anisotropic);

            regex = new Regex(@"AlphaBlendEnable", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.AlphaBlendEnable, regex);
            Tokens.Add(TokenType.AlphaBlendEnable);

            regex = new Regex(@"SrcBlend", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.SrcBlend, regex);
            Tokens.Add(TokenType.SrcBlend);

            regex = new Regex(@"DestBlend", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.DestBlend, regex);
            Tokens.Add(TokenType.DestBlend);

            regex = new Regex(@"BlendOp", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.BlendOp, regex);
            Tokens.Add(TokenType.BlendOp);

            regex = new Regex(@"ColorWriteEnable", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.ColorWriteEnable, regex);
            Tokens.Add(TokenType.ColorWriteEnable);

            regex = new Regex(@"ZEnable", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.ZEnable, regex);
            Tokens.Add(TokenType.ZEnable);

            regex = new Regex(@"ZWriteEnable", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.ZWriteEnable, regex);
            Tokens.Add(TokenType.ZWriteEnable);

            regex = new Regex(@"DepthBias", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.DepthBias, regex);
            Tokens.Add(TokenType.DepthBias);

            regex = new Regex(@"CullMode", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.CullMode, regex);
            Tokens.Add(TokenType.CullMode);

            regex = new Regex(@"FillMode", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.FillMode, regex);
            Tokens.Add(TokenType.FillMode);

            regex = new Regex(@"MultiSampleAntiAlias", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.MultiSampleAntiAlias, regex);
            Tokens.Add(TokenType.MultiSampleAntiAlias);

            regex = new Regex(@"SlopeScaleDepthBias", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.SlopeScaleDepthBias, regex);
            Tokens.Add(TokenType.SlopeScaleDepthBias);

            regex = new Regex(@"Red", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Red, regex);
            Tokens.Add(TokenType.Red);

            regex = new Regex(@"Green", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Green, regex);
            Tokens.Add(TokenType.Green);

            regex = new Regex(@"Blue", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Blue, regex);
            Tokens.Add(TokenType.Blue);

            regex = new Regex(@"Alpha", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Alpha, regex);
            Tokens.Add(TokenType.Alpha);

            regex = new Regex(@"All", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.All, regex);
            Tokens.Add(TokenType.All);

            regex = new Regex(@"Cw", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Cw, regex);
            Tokens.Add(TokenType.Cw);

            regex = new Regex(@"Ccw", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Ccw, regex);
            Tokens.Add(TokenType.Ccw);

            regex = new Regex(@"Solid", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Solid, regex);
            Tokens.Add(TokenType.Solid);

            regex = new Regex(@"WireFrame", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.WireFrame, regex);
            Tokens.Add(TokenType.WireFrame);

            regex = new Regex(@"Add", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Add, regex);
            Tokens.Add(TokenType.Add);

            regex = new Regex(@"Subtract", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Subtract, regex);
            Tokens.Add(TokenType.Subtract);

            regex = new Regex(@"RevSubtract", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.RevSubtract, regex);
            Tokens.Add(TokenType.RevSubtract);

            regex = new Regex(@"Min", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Min, regex);
            Tokens.Add(TokenType.Min);

            regex = new Regex(@"Max", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Max, regex);
            Tokens.Add(TokenType.Max);

            regex = new Regex(@"Zero", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.Zero, regex);
            Tokens.Add(TokenType.Zero);

            regex = new Regex(@"One", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.One, regex);
            Tokens.Add(TokenType.One);

            regex = new Regex(@"SrcColor", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.SrcColor, regex);
            Tokens.Add(TokenType.SrcColor);

            regex = new Regex(@"InvSrcColor", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.InvSrcColor, regex);
            Tokens.Add(TokenType.InvSrcColor);

            regex = new Regex(@"SrcAlpha", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.SrcAlpha, regex);
            Tokens.Add(TokenType.SrcAlpha);

            regex = new Regex(@"InvSrcAlpha", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.InvSrcAlpha, regex);
            Tokens.Add(TokenType.InvSrcAlpha);

            regex = new Regex(@"DestAlpha", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.DestAlpha, regex);
            Tokens.Add(TokenType.DestAlpha);

            regex = new Regex(@"InvDestAlpha", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.InvDestAlpha, regex);
            Tokens.Add(TokenType.InvDestAlpha);

            regex = new Regex(@"DestColor", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.DestColor, regex);
            Tokens.Add(TokenType.DestColor);

            regex = new Regex(@"InvDestColor", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.InvDestColor, regex);
            Tokens.Add(TokenType.InvDestColor);

            regex = new Regex(@"SrcAlphaSat", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.SrcAlphaSat, regex);
            Tokens.Add(TokenType.SrcAlphaSat);

            regex = new Regex(@"BlendFactor", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.BlendFactor, regex);
            Tokens.Add(TokenType.BlendFactor);

            regex = new Regex(@"InvBlendFactor", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Patterns.Add(TokenType.InvBlendFactor, regex);
            Tokens.Add(TokenType.InvBlendFactor);


        }

        public void Init(string input)
        {
            Init(input, "");
        }

        public void Init(string input, string fileName)
        {
            this.Input = input;
            StartPos = 0;
            EndPos = 0;
            CurrentFile = fileName;
            CurrentLine = 1;
            CurrentColumn = 1;
            CurrentPosition = 0;
            LookAheadToken = null;
        }

        public Token GetToken(TokenType type)
        {
            Token t = new Token(this.StartPos, this.EndPos);
            t.Type = type;
            return t;
        }

         /// <summary>
        /// executes a lookahead of the next token
        /// and will advance the scan on the input string
        /// </summary>
        /// <returns></returns>
        public Token Scan(params TokenType[] expectedtokens)
        {
            Token tok = LookAhead(expectedtokens); // temporarely retrieve the lookahead
            LookAheadToken = null; // reset lookahead token, so scanning will continue
            StartPos = tok.EndPos;
            EndPos = tok.EndPos; // set the tokenizer to the new scan position
            CurrentLine = tok.Line + (tok.Text.Length - tok.Text.Replace("\n", "").Length);
            CurrentFile = tok.File;
            return tok;
        }

        /// <summary>
        /// returns token with longest best match
        /// </summary>
        /// <returns></returns>
        public Token LookAhead(params TokenType[] expectedtokens)
        {
            int i;
            int startpos = StartPos;
            int endpos = EndPos;
            int currentline = CurrentLine;
            string currentFile = CurrentFile;
            Token tok = null;
            List<TokenType> scantokens;


            // this prevents double scanning and matching
            // increased performance
            if (LookAheadToken != null 
                && LookAheadToken.Type != TokenType._UNDETERMINED_ 
                && LookAheadToken.Type != TokenType._NONE_) return LookAheadToken;

            // if no scantokens specified, then scan for all of them (= backward compatible)
            if (expectedtokens.Length == 0)
                scantokens = Tokens;
            else
            {
                scantokens = new List<TokenType>(expectedtokens);
                scantokens.AddRange(SkipList);
            }

            do
            {

                int len = -1;
                TokenType index = (TokenType)int.MaxValue;
                string input = Input.Substring(startpos);

                tok = new Token(startpos, endpos);

                for (i = 0; i < scantokens.Count; i++)
                {
                    Regex r = Patterns[scantokens[i]];
                    Match m = r.Match(input);
                    if (m.Success && m.Index == 0 && ((m.Length > len) || (scantokens[i] < index && m.Length == len )))
                    {
                        len = m.Length;
                        index = scantokens[i];  
                    }
                }

                if (index >= 0 && len >= 0)
                {
                    tok.EndPos = startpos + len;
                    tok.Text = Input.Substring(tok.StartPos, len);
                    tok.Type = index;
                }
                else if (tok.StartPos == tok.EndPos)
                {
                    if (tok.StartPos < Input.Length)
                        tok.Text = Input.Substring(tok.StartPos, 1);
                    else
                        tok.Text = "EOF";
                }

                // Update the line and column count for error reporting.
                tok.File = currentFile;
                tok.Line = currentline;
                if (tok.StartPos < Input.Length)
                    tok.Column = tok.StartPos - Input.LastIndexOf('\n', tok.StartPos);

                if (SkipList.Contains(tok.Type))
                {
                    startpos = tok.EndPos;
                    endpos = tok.EndPos;
                    currentline = tok.Line + (tok.Text.Length - tok.Text.Replace("\n", "").Length);
                    currentFile = tok.File;
                    Skipped.Add(tok);
                }
                else
                {
                    // only assign to non-skipped tokens
                    tok.Skipped = Skipped; // assign prior skips to this token
                    Skipped = new List<Token>(); //reset skips
                }

                // Check to see if the parsed token wants to 
                // alter the file and line number.
                if (tok.Type == FileAndLine)
                {
                    var match = Patterns[tok.Type].Match(tok.Text);
                    var fileMatch = match.Groups["File"];
                    if (fileMatch.Success)
                        currentFile = fileMatch.Value.Replace("\\\\", "\\");
                    var lineMatch = match.Groups["Line"];
                    if (lineMatch.Success)
                        currentline = int.Parse(lineMatch.Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                }
            }
            while (SkipList.Contains(tok.Type));

            LookAheadToken = tok;
            return tok;
        }
    }

    #endregion

    #region Token

    public enum TokenType
    {

            //Non terminal tokens:
            _NONE_  = 0,
            _UNDETERMINED_= 1,

            //Non terminal tokens:
            Start   = 2,
            Technique_Declaration= 3,
            FillMode_Solid= 4,
            FillMode_WireFrame= 5,
            FillModes= 6,
            CullMode_None= 7,
            CullMode_Cw= 8,
            CullMode_Ccw= 9,
            CullModes= 10,
            Colors_None= 11,
            Colors_Red= 12,
            Colors_Green= 13,
            Colors_Blue= 14,
            Colors_Alpha= 15,
            Colors_All= 16,
            Colors_Boolean= 17,
            Colors  = 18,
            ColorsMasks= 19,
            Blend_Zero= 20,
            Blend_One= 21,
            Blend_SrcColor= 22,
            Blend_InvSrcColor= 23,
            Blend_SrcAlpha= 24,
            Blend_InvSrcAlpha= 25,
            Blend_DestAlpha= 26,
            Blend_InvDestAlpha= 27,
            Blend_DestColor= 28,
            Blend_InvDestColor= 29,
            Blend_SrcAlphaSat= 30,
            Blend_BlendFactor= 31,
            Blend_InvBlendFactor= 32,
            Blends  = 33,
            BlendOp_Add= 34,
            BlendOp_Subtract= 35,
            BlendOp_RevSubtract= 36,
            BlendOp_Min= 37,
            BlendOp_Max= 38,
            BlendOps= 39,
            Render_State_CullMode= 40,
            Render_State_FillMode= 41,
            Render_State_AlphaBlendEnable= 42,
            Render_State_SrcBlend= 43,
            Render_State_DestBlend= 44,
            Render_State_BlendOp= 45,
            Render_State_ColorWriteEnable= 46,
            Render_State_DepthBias= 47,
            Render_State_SlopeScaleDepthBias= 48,
            Render_State_ZEnable= 49,
            Render_State_ZWriteEnable= 50,
            Render_State_MultiSampleAntiAlias= 51,
            Render_State_Expression= 52,
            Pass_Declaration= 53,
            VertexShader_Pass_Expression= 54,
            PixelShader_Pass_Expression= 55,
            AddressMode_Clamp= 56,
            AddressMode_Wrap= 57,
            AddressMode_Mirror= 58,
            AddressMode= 59,
            TextureFilter_None= 60,
            TextureFilter_Linear= 61,
            TextureFilter_Point= 62,
            TextureFilter_Anisotropic= 63,
            TextureFilter= 64,
            Sampler_State_Texture= 65,
            Sampler_State_MinFilter= 66,
            Sampler_State_MagFilter= 67,
            Sampler_State_MipFilter= 68,
            Sampler_State_Filter= 69,
            Sampler_State_AddressU= 70,
            Sampler_State_AddressV= 71,
            Sampler_State_AddressW= 72,
            Sampler_State_MaxMipLevel= 73,
            Sampler_State_MaxAnisotropy= 74,
            Sampler_State_MipLodBias= 75,
            Sampler_State_Expression= 76,
            Sampler_Register_Expression= 77,
            Sampler_Declaration= 78,

            //Terminal tokens:
            BlockComment= 79,
            Comment = 80,
            Whitespace= 81,
            LinePragma= 82,
            Pass    = 83,
            Technique= 84,
            Sampler = 85,
            SamplerState= 86,
            VertexShader= 87,
            PixelShader= 88,
            Register= 89,
            Boolean = 90,
            Number  = 91,
            Identifier= 92,
            OpenBracket= 93,
            CloseBracket= 94,
            Equals  = 95,
            Colon   = 96,
            Comma   = 97,
            Semicolon= 98,
            Or      = 99,
            OpenParenthesis= 100,
            CloseParenthesis= 101,
            OpenSquareBracket= 102,
            CloseSquareBracket= 103,
            LessThan= 104,
            GreaterThan= 105,
            Compile = 106,
            ShaderModel= 107,
            Code    = 108,
            EndOfFile= 109,
            MinFilter= 110,
            MagFilter= 111,
            MipFilter= 112,
            Filter  = 113,
            Texture = 114,
            AddressU= 115,
            AddressV= 116,
            AddressW= 117,
            MaxAnisotropy= 118,
            MaxMipLevel= 119,
            MipLodBias= 120,
            Clamp   = 121,
            Wrap    = 122,
            Mirror  = 123,
            None    = 124,
            Linear  = 125,
            Point   = 126,
            Anisotropic= 127,
            AlphaBlendEnable= 128,
            SrcBlend= 129,
            DestBlend= 130,
            BlendOp = 131,
            ColorWriteEnable= 132,
            ZEnable = 133,
            ZWriteEnable= 134,
            DepthBias= 135,
            CullMode= 136,
            FillMode= 137,
            MultiSampleAntiAlias= 138,
            SlopeScaleDepthBias= 139,
            Red     = 140,
            Green   = 141,
            Blue    = 142,
            Alpha   = 143,
            All     = 144,
            Cw      = 145,
            Ccw     = 146,
            Solid   = 147,
            WireFrame= 148,
            Add     = 149,
            Subtract= 150,
            RevSubtract= 151,
            Min     = 152,
            Max     = 153,
            Zero    = 154,
            One     = 155,
            SrcColor= 156,
            InvSrcColor= 157,
            SrcAlpha= 158,
            InvSrcAlpha= 159,
            DestAlpha= 160,
            InvDestAlpha= 161,
            DestColor= 162,
            InvDestColor= 163,
            SrcAlphaSat= 164,
            BlendFactor= 165,
            InvBlendFactor= 166
    }

    public class Token
    {
        private string file;
        private int line;
        private int column;
        private int startpos;
        private int endpos;
        private string text;
        private object value;

        // contains all prior skipped symbols
        private List<Token> skipped;

        public string File { 
            get { return file; } 
            set { file = value; }
        }

        public int Line { 
            get { return line; } 
            set { line = value; }
        }

        public int Column {
            get { return column; } 
            set { column = value; }
        }

        public int StartPos { 
            get { return startpos;} 
            set { startpos = value; }
        }

        public int Length { 
            get { return endpos - startpos;} 
        }

        public int EndPos { 
            get { return endpos;} 
            set { endpos = value; }
        }

        public string Text { 
            get { return text;} 
            set { text = value; }
        }

        public List<Token> Skipped { 
            get { return skipped;} 
            set { skipped = value; }
        }
        public object Value { 
            get { return value;} 
            set { this.value = value; }
        }

        [XmlAttribute]
        public TokenType Type;

        public Token()
            : this(0, 0)
        {
        }

        public Token(int start, int end)
        {
            Type = TokenType._UNDETERMINED_;
            startpos = start;
            endpos = end;
            Text = ""; // must initialize with empty string, may cause null reference exceptions otherwise
            Value = null;
        }

        public void UpdateRange(Token token)
        {
            if (token.StartPos < startpos) startpos = token.StartPos;
            if (token.EndPos > endpos) endpos = token.EndPos;
        }

        public override string ToString()
        {
            if (Text != null)
                return Type.ToString() + " '" + Text + "'";
            else
                return Type.ToString();
        }
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

public class RubyText : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI         Text;

    [SerializeField]
    RectTransform           TextRect;

    [SerializeField]
    TextMeshProUGUI         Ruby;

    [SerializeField, Tooltip("１文字、または文章全体を表示する時間（秒）. ０の場合一度に表示"), Range(0, 1)]
    public float            AutoForwardSpeed;

    [SerializeField, Tooltip("true/１度に全文章を表示、false/１文字ずつ表示")]
    public bool             IsDrawAtOnce;

    /// <summary>
    /// テキスト描画終了
    /// </summary>
    public System.Action    TextDrawFinished;

    class TextRuby
    {
        public int          TextPosition;
        public string       Word;
        public string       RubyWord;

        TextMeshProUGUI     parentText;
        TextMeshProUGUI     ruby;
        RectTransform       rubyRect;

        TMP_CharacterInfo[] characterInfos;
        int                 posTop;
        int                 posBtm;

        float               rubyWidth;

        /// <summary>
        /// .ctor
        /// </summary>
        public TextRuby(int no, TextMeshProUGUI rubyBase, TextMeshProUGUI _parentText)
        {
            parentText = _parentText;
            rubyWidth  = 0;

            ruby = Instantiate(rubyBase, parentText.transform);
            ruby.name = $"ruby {no}";

            rubyRect = ruby.GetComponent<RectTransform>();
            rubyRect.SetHeight(ruby.fontSize);
        }

        /// <summary>
        /// SetActive
        /// </summary>
        public void SetActive(bool active)
        {
            ruby.SetActive(active);
        }

        /// <summary>
        /// ルビワードの設定、表示範囲の設定
        /// </summary>
        public void Refresh()
        {
            if (RubyWord == null)
            {
                return;
            }

            float textWidth = (characterInfos[posBtm].topRight.x - characterInfos[posTop].topLeft.x);

            ruby.SetActive(false);
            ruby.SetText(RubyWord);
            if (RubyWord.Length == 1)
            {
                // center
                ruby.alignment = TextAlignmentOptions.Bottom;
            }
            else
            {
                // 程よく間を空ける
                ruby.alignment = TextAlignmentOptions.BottomFlush;
            }

            rubyWidth = ruby.preferredWidth;
            if (rubyWidth < textWidth * 0.9f)
            {
                rubyRect.SetWidth(textWidth * 0.9f);
            }
            else
            {
                rubyRect.SetWidth(rubyWidth);
            }
            rubyRect.SetHeight(ruby.preferredHeight);
        }

        /// <summary>
        /// クリア
        /// </summary>
        public void Clear()
        {
            ruby.SetActive(false);
            ruby.SetText("");

            Word = null;
            RubyWord = null;
        }

        /// <summary>
        /// αの更新
        /// </summary>
        /// <param name="messagePosition">現在の最終文字表示位置</param>
        /// <param name="a">最終文字のα</param>
        public void UpdateAlpha(int messagePosition, float a)
        {
            if (posBtm > messagePosition)
            {
                ruby.color = new Color(ruby.color.r, ruby.color.g, ruby.color.b, 0);
            }
            else
            if (posBtm < messagePosition)
            {
                ruby.color = new Color(ruby.color.r, ruby.color.g, ruby.color.b, 1);
            }
            else
            {
                ruby.color = new Color(ruby.color.r, ruby.color.g, ruby.color.b, a);
            }
        }

        /// <summary>
        /// αの更新
        /// </summary>
        /// <param name="a">最終文字のα</param>
        public void UpdateAlpha(float a)
        {
            ruby.color = new Color(ruby.color.r, ruby.color.g, ruby.color.b, a);
        }

        /// <summary>
        /// ルビフォントサイズの設定
        /// </summary>
        /// <param name="fontSize">本文フォントのサイズ</param>
        public void SetFontSize(float fontSize)
        {
            ruby.fontSize    = 
            ruby.fontSizeMax = fontSize/2;
        }

        /// <summary>
        /// 計算済みの文字情報を設定
        /// </summary>
        /// <param name="characterInfos">計算済みの文字情報</param>
        /// <param name="posTop">ルビ先頭文字位置</param>
        /// <param name="posBtm">ルビ終了文字位置</param>
        public void SetTmpInfo(TMP_CharacterInfo[] characterInfos, int posTop, int posBtm)
        {
            this.characterInfos = characterInfos;
            this.posTop         = posTop;
            this.posBtm         = posBtm;

            var top = this.characterInfos[this.posTop];
            var btm = this.characterInfos[posBtm];

            float y = top.ascender;
            float x = (top.topLeft.x + btm.topRight.x) / 2;

            rubyRect.SetXY(x, y);

            float r = (float)this.characterInfos[this.posBtm].color.r / 255;
            float g = (float)this.characterInfos[this.posBtm].color.g / 255;
            float b = (float)this.characterInfos[this.posBtm].color.b / 255;

            ruby.color = new Color(r, g, b, 0);
        }
    }

    /// <summary>
    /// 前回値. 現在値と比較し、違いがあったらそれぞれを更新
    /// </summary>
    class UpdateComparer
    {
        public string Message;
        public int    Position;
        public float  Alpha;
        public float  W, H;
        public bool   EnableWordWrapping;

        public UpdateComparer(TextMeshProUGUI text)
        {
            Clear(text);
        }

        public void Clear(TextMeshProUGUI text)
        {
            Message  = null;
            Position = 0;
            Alpha    = 0;
            W        = 0;
            H        = 0;
            EnableWordWrapping = text.enableWordWrapping == true;
//            EnableWordWrapping = text.textWrappingMode != TextWrappingModes.NoWrap;
        }
    }

    List<TextRuby>              textRubys;
    int                         textRubyCount;
    UpdateComparer              updateComparer;
    TMP_CharacterInfo[]         cinfos;

    float                       fontSizeMax;
    string                      message;
    List<int>                   positionIndexes;

    int                         position;
    float                       alpha;

    Coroutine                   co_text;
    Coroutine                   co_auto;

    /// <summary>
    /// awake
    /// </summary>
    void Awake()
    {
        Ruby.SetActive(false);

        textRubys = new List<TextRuby>();
        updateComparer = new UpdateComparer(Text);

        fontSizeMax = Text.fontSizeMax;
    }

    /// <summary>
    /// update
    /// </summary>
    void Update()
    {
        bool update = false;

        if (updateComparer.W != TextRect.GetWidth() || updateComparer.H != TextRect.GetHeight())
        {
            updateComparer.W = TextRect.GetWidth();
            updateComparer.H = TextRect.GetHeight();
            update = true;
        }

        bool wrapping = Text.enableWordWrapping == true;
//        bool wrapping = Text.textWrappingMode != TextWrappingModes.NoWrap;

        if (updateComparer.EnableWordWrapping != wrapping)
        {
            updateComparer.EnableWordWrapping = wrapping;
            update = true;
        }

        if (update == true)
        {
            // 再描画
            updateComparer.Position = 0;
            updateComparer.Alpha = 0;

            this.StartSingleCoroutine(ref co_text, textDrawing(false));
        }
    }

    /// <summary>
    /// on destroy
    /// </summary>
    void OnDestroy()
    {
        this.StopSingleCoroutine(ref co_auto);
        this.StopSingleCoroutine(ref co_text);
    }

    private void OnDisable()
    {
        Debug.Log($"Disable {message}");
    }

    /// <summary>
    /// 文字自動送りの速度を設定する
    /// </summary>
    /// <param name="_secPerWord">ｎ秒で１文字表示</param>
    public void SetAutoForwardSpeed(float _secPerWord)
    {
        AutoForwardSpeed = _secPerWord;
    }

    /// <summary>
    /// 文字自動送り
    /// </summary>
    public void StartAutoForward()
    {
        if (positionIndexes == null)
        {
            return;
        }

        this.StartSingleCoroutine(ref co_auto, autoForward());
    }

    /// <summary>
    /// 文章の表示位置を強制する
    /// </summary>
    /// <param name="pos">整数：最終表示文字位置、少数：α</param>
    public void ForceTextPosition(float pos)
    {
        this.StopSingleCoroutine(ref co_auto);

        setTextPosition(pos);
    }

    /// <summary>
    /// 文字を全て表示する
    /// </summary>
    /// <param name="pos">整数：最終表示文字位置、少数：α</param>
    public void ForceTextDrawAll()
    {
        ForceTextPosition(GetTextLength());
    }

    /// <summary>
    /// テキストをクリア
    /// </summary>
    public void ResetText()
    {
        SetText(null);
    }

    /// <summary>
    /// 表示する文章の設定. {漢字:かんじ} の書式でルビを表現する
    /// </summary>
    /// <param name="_message">表示する文章</param>
    public void SetText(string _message)
    {
        if (_message == null)
        {
            _message = "";
        }

        message = _message;

        Text.SetText("");
        Text.fontSizeMax = fontSizeMax;

        textRubys.ForEach( ruby => ruby.Clear() );
        textRubyCount = 0;

        position = 0;
        alpha = 0;
        positionIndexes = null;

        updateComparer.Clear(Text);

        if (string.IsNullOrEmpty(_message) == true)
        {
            return;
        }

        // <> ～ </> コマンドなし
        var notagMessage = Regex.Replace(message, "<[^<|>]+>", "");

        for ( ; ; )
        {
            int top = notagMessage.IndexOf("{");
            int btm = notagMessage.IndexOf("}");

            if (top < 0 || btm < 0)
            {
                break;
            }

            string   command = notagMessage.Substring(top, btm-top+1).Replace("{", "").Replace("}", "");

            try
            {
                // コマンド
                string[] coms    = command.Split(':');

                if (textRubyCount+1 > textRubys.Count)
                {
                    // なければバッファ拡張
                    textRubys.Add(new TextRuby(textRubyCount, Ruby, Text));
                }

                var ruby = textRubys[textRubyCount++];
                ruby.TextPosition = top;
                ruby.Word         = coms[0];
                ruby.RubyWord     = coms[1];

                //Debug.Log($"{top} {string.Join(",", coms)}");

                notagMessage      = notagMessage.Remove(top, btm-top+1).Insert(top, coms[0]);
            }
            catch
            {
                Debug.LogError($"Unformat RubyWord: {command}");
                break;
            }
        }

        // {} コマンドなしの、TextMeshProUGUI に渡すテキスト
        message = Regex.Replace(message, ":[^\\}]+\\}", "").Replace("{", "");

        // ｍ文字目が文字列ｎ番目から表示されることを確認するリスト
        // （タグも加味するため ｍ≠ｎ）
        positionIndexes = new List<int>();
        for (int i = 0; i < message.Length; i++)
        {
            if (message[i] == '<')
            {
                i = message.IndexOf('>', i);
                continue;
            }
            positionIndexes.Add(i);
        }

        this.StartSingleCoroutine(ref co_text, textDrawing(true));

//Debug.Log($"{positionIndexes.Count} {message}");
    }

    /// <summary>
    /// テキストの文字数を取得。タグは除く
    /// </summary>
    public int GetTextLength()
    {
        return positionIndexes == null ? 0 : positionIndexes.Count;
    }

    /// <summary>
    /// テキスト描画中は true、それ以外は false
    /// </summary>
    public bool CheckTextDrawing()
    {
        return co_text != null;
    }

    /// <summary>
    /// RectXY
    /// </summary>
    public void SetXY(float x, float y)
    {
        SetX(x);
        SetY(y);
    }

    /// <summary>
    /// RectX
    /// </summary>
    public void SetX(float x)
    {
        TextRect.SetX(x);
    }

    /// <summary>
    /// RectY
    /// </summary>
    public void SetY(float y)
    {
        TextRect.SetY(y);
    }

    /// <summary>
    /// RectWH
    /// </summary>
    public void SetWH(float width, float height)
    {
        SetWidth(width);
        SetHeight(height);
    }

    /// <summary>
    /// RectW
    /// </summary>
    public void SetWidth(float width)
    {
        TextRect.SetWidth(width);
        updateComparer.W = width;
    }

    /// <summary>
    /// RectH
    /// </summary>
    public void SetHeight(float height)
    {
        TextRect.SetHeight(height);
        updateComparer.H = height;
    }

    /// <summary>
    /// フォントの AutoSize を設定する
    /// </summary>
    public void SetFontAutoSize(float min, float max)
    {
        Text.enableAutoSizing = true;
        Text.fontSizeMin = min;
        Text.fontSizeMax = max;
        Text.fontSize    = max;
        fontSizeMax      = max;
    }

    /// <summary>
    /// フォントサイズを設定する
    /// </summary>
    public void SetFontSize(float size)
    {
        Text.fontSizeMax = size;
        Text.fontSize    = size;
        fontSizeMax      = size;
    }

    /// <summary>
    /// カラーを設定する
    /// </summary>
    public void SetColor(Color color)
    {
        Text.color = color;
    }

    /// <summary>
    /// 文字寄せのタイプを設定
    /// </summary>
    public void SetAlignment(TextAlignmentOptions alignment)
    {
        Text.alignment = alignment;
    }

    /// <summary>
    /// 文字間を設定
    /// </summary>
    public void SetCharacterSpacing(float spacing)
    {
        Text.characterSpacing = spacing;
    }

    /// <summary>
    /// 行間を設定
    /// </summary>
    public void SetLineSpacing(float spacing)
    {
        Text.lineSpacing = spacing;
    }

    /// <summary>
    /// TextMeshProUGUI を返す
    /// </summary>
    public TextMeshProUGUI GetUIText()
    {
        return Text;
    }

    /// <summary>
    /// 文字自動送り
    /// </summary>
    /// <returns></returns>
    IEnumerator autoForward()
    {
        int   max = positionIndexes.Count;
        float time = 0;

        if (AutoForwardSpeed == 0)
        {
            setTextPosition(max);
            yield break;
        }

        yield return null;

        for ( ; time < max; )
        {
            time += Time.deltaTime * (1.0f / AutoForwardSpeed);
            
            setTextPosition(time);

            yield return null;
        }

        co_auto = null;
    }

    /// <summary>
    /// テキスト描画
    /// </summary>
    IEnumerator textDrawing(bool calculate)
    {
        textRubys.ForEach( ruby => ruby.Clear() );

        if (calculate == true)
        {
            Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, 0);

            // 一旦テキストが見えない状態で全テキストを描画する（文字情報を取得するため）
            Text.SetText(message);

        }

        // １フレーム経過しないと文字情報が更新されない
        yield return null;

        // 全文字が表示された状態のフォントサイズがフォント最大サイズとする
        // （これをしておかないと、文字が少ない時バカでかい文字になるなど、不安定なテキスト描画になる）
        Text.fontSizeMax = Text.fontSize;

        // 出そろった文字情報を元にルビを設定
        refreshRuby();

        Text.SetText("");

        // カラーを戻す
        Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, 1);

        while (true)
        {
            var msg = Text.text;

//DDisp.Log($"{position} {alpha}");
            // 文章が変わったり、表示位置が変化
            if (updateComparer.Message != message || updateComparer.Position != position)
            {
                if (positionIndexes != null && position >= 0 && position < positionIndexes.Count)
                {
                    int    ia   = (int)(255 * alpha);
                    string taga = $"<alpha=#{ia.ToString("x2")}>";

                    if (IsDrawAtOnce == true)
                    {
                        msg = taga + message;
                        msg = Regex.Replace(msg, $"(?<tag><color=[^>]+>)", "${tag}" + taga);
                        msg = msg.Replace("</color>", "</color>" + taga);
                        refreshRubyAlpha(alpha);
                    }
                    else
                    {
                        msg = message.Substring(0, positionIndexes[position]+1);

                        if (msg.Length >= 1)
                        {
                            msg = msg.Insert(positionIndexes[position], taga);

                            refreshRubyAlpha(alpha);
                        }
                    }
                }

                updateComparer.Message = message;
                updateComparer.Position = position;
                updateComparer.Alpha = alpha;
            }
            else
            // 最終文字のαが変化
            if (updateComparer.Alpha != alpha)
            {
                if (msg.Length > 1)
                {
                    int ia = (int)(255 * alpha);
                    msg = Regex.Replace(msg, $"<alpha=[^>]+>", $"<alpha=#{ia.ToString("x2")}>");

                    refreshRubyAlpha(alpha);
                }

                updateComparer.Alpha = alpha;
            }

            if (Text.text != msg)
            {
//Debug.Log($"{msg}");
                Text.SetText(msg);
            }

            if (alpha == 1 && this.position == GetTextLength()-1)
            {
                break;
            }

            yield return null;
        }

        co_text = null;

        TextDrawFinished?.Invoke();
    }

    /// <summary>
    /// ルビの再設定
    /// </summary>
    void refreshRuby()
    {
        for (int i = 0; i < textRubys.Count; i++)
        {
            textRubys[i].SetActive(false);
        }

        // 文字描画後の文字情報（表示位置やカラーなど）
        cinfos = Text.GetTextInfo(Text.text).characterInfo;

        for (int i = 0; i < textRubys.Count; i++)
        {
            var ruby    = textRubys[i];
            if (ruby.Word == null)
            {
                continue;
            }

            int posTop  = ruby.TextPosition;
            int posBtm;

            var infoTop = cinfos[posTop];

            // ルビを振る文字列のうち、同じ高さの終端文字を検索する
            // （文字列が自動改行などで２行にまたがってしまう問題の対策）
            for (posBtm = ruby.TextPosition + ruby.Word.Length - 1; ; posBtm--)
            {
                var info = cinfos[posBtm];
                if (infoTop.ascender == info.ascender)
                {
                    break;
                }
            }

            ruby.SetTmpInfo(cinfos, posTop, posBtm);
            ruby.SetFontSize(Text.fontSize);
            ruby.Refresh();
            ruby.SetActive(true);
        }

        refreshRubyAlpha(0);
    }

    /// <summary>
    /// ルビの表示（α更新）
    /// </summary>
    void refreshRubyAlpha(float alpha)
    {
        if (IsDrawAtOnce == true)
        {
            textRubys.ForEach( ruby => ruby.UpdateAlpha(alpha) );
        }
        else
        {
            textRubys.ForEach( ruby => ruby.UpdateAlpha(position, alpha) );
        }
    }

    /// <summary>
    /// 文字の表示位置設定
    /// </summary>
    /// <param name="pos">整数：最終表示文字位置、少数：α</param>
    void setTextPosition(float pos)
    {
        int count = GetTextLength();

        if (IsDrawAtOnce == true)
        {
            this.position = count - 1;
            alpha = pos;
            if (alpha > 1)
            {
                alpha = 1;
            }
        }
        else
        {
            if (pos < 0)
            {
                this.position = 0;
                alpha         = 0;
            }
            else
            if (pos >= count)
            {
                this.position = count - 1;
                alpha         = 1;
            }
            else
            {
                this.position = (int)pos;
                alpha         = pos - this.position;
            }
        }
    }

}

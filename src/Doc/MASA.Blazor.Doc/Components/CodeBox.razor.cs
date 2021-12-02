﻿using BlazorComponent;
using MASA.Blazor.Doc.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MASA.Blazor.Doc.Components;

public partial class CodeBox
{
    private const int Await = 2000;

    private static string _githubUrlTemplate =
        "https://github.com/BlazorComponent/MASA.Blazor/blob/main/src/Doc/MASA.Blazor.Doc/{0}.razor";

    private readonly static(string type, string lang) Template = ("template", "html");
    private readonly static(string type, string lang) Code = ("code", "csharp");
    private readonly static(string type, string lang) Style = ("style", "css");

    private readonly Dictionary<(string type, string lang), string> _items = new()
    {
        { Template, null },
        { Code, null },
        { Style, null },
    };

    private StringNumber _activeItem;
    private bool _clicked;
    private bool _expend;
    private bool _showComponent;

    private RenderFragment Component { get; set; }

    [Inject]
    public IJSRuntime Js { get; set; }

    [Parameter]
    public string ComponentName { get; set; }

    [Parameter]
    public DemoItemModel Demo { get; set; }

    [Parameter]
    public int Index { get; set; }

    private string GithubUrlHref { get; set; }

    protected override void OnInitialized()
    {
        _showComponent = Index < 2;

        if (Demo.Type == null) return;

        Component = Service.GetShowCase(Demo.Type);

        var path = Demo.Type.Replace(".", "/");
        GithubUrlHref = string.Format(_githubUrlTemplate, path);

        if (Demo.Code == null) return;

        var styleFrom = Demo.Code.IndexOf("<style", StringComparison.Ordinal);
        var styleTo = Demo.Code.IndexOf("</style>", StringComparison.Ordinal) + "</style>".Length;

        var code = Demo.Code;
        if (styleFrom > -1 && styleTo > -1)
        {
            var styleContent = Demo.Code.Substring(styleFrom, styleTo - styleFrom);
            _items[Style] = styleContent;

            code = code.Replace(styleContent, "");
        }

        var index = code.IndexOf("@code");
        if (index > -1)
        {
            _items[Template] = code.Substring(0, index).Trim();
            _items[Code] = code.Substring(index).Trim();
        }
        else
        {
            _items[Template] = code.Trim();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!_showComponent)
            {
                await Task.Delay(Index * 16);
                _showComponent = true;
                StateHasChanged();
            }
        }
    }

    private async Task Copy(string text)
    {
        _clicked = true;

        await Js.InvokeVoidAsync(JsInteropConstants.Copy, text);

        await Task.Delay(Await);

        _clicked = false;
    }
}
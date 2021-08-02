﻿using System.Threading.Tasks;
using BlazorComponent;
using MASA.Blazor.Helpers;
using Microsoft.AspNetCore.Components;

namespace MASA.Blazor
{
    public partial class MCheckbox : BCheckbox, IThemeable
    {

        [Parameter]
        public bool Dark { get; set; }

        [Parameter]
        public bool Light { get; set; }

        [CascadingParameter]
        public IThemeable Themeable { get; set; }

        public bool IsDark
        {
            get
            {
                if (Dark)
                {
                    return true;
                }

                if (Light)
                {
                    return false;
                }

                return Themeable != null && Themeable.IsDark;
            }
        } 

        protected override Task OnInitializedAsync()
        {
            UncheckIconContent = RenderIcon("mdi-checkbox-blank-outline", Color, Disabled, Dark);
            CheckedIconContent = RenderIcon("mdi-checkbox-marked", Color ?? "primary", Disabled, Dark);
            IndeterminateIconContent = RenderIcon("mdi-minus-box", Color, Disabled, Dark);

            AnimationContent = RenderRipple(Color);

            return base.OnInitializedAsync();
        }

        private RenderFragment RenderIcon(string icon, string color, bool disabled, bool dark) => builder =>
        {
            int sequence = 0;
            builder.OpenComponent(sequence++, typeof(MIcon));

            if (!disabled)
            {
                builder.AddAttribute(sequence++, nameof(MIcon.Color), color);
                builder.AddAttribute(sequence++, nameof(MIcon.Dark), dark);
            }

            builder.AddAttribute(sequence++, nameof(MIcon.ChildContent), (RenderFragment)((builder) => builder.AddContent(sequence++, icon)));
            builder.CloseComponent();
        };

        private RenderFragment RenderRipple(string color) => builder =>
        {
            int sequence = 0;
            builder.OpenElement(sequence++, "div");

            var (color_css, color_style) = ColorHelper.ToCss(color);
            builder.AddAttribute(sequence++, "class", $"m-input--selection-controls__ripple {color_css}");

            builder.AddAttribute(sequence++, "style", color_style);

            builder.CloseElement();
        };

        protected override void SetComponentClass()
        {
            CssProvider
                .AsProvider<BCheckbox>()
                .Apply(cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input")
                        .Add("m-input--selection-controls")
                        .Add("m-input--checkbox")
                        .AddIf("m-input--is-disabled", () => Disabled)
                        .AddTheme(IsDark);
                })
                .Apply("control", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input__control");
                })
                .Apply("slot", cssBuilder =>
                {
                    cssBuilder
                       .Add("m-input__slot");
                })
                .Apply("input", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-input--selection-controls__input");
                })
                .Apply("label", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-label");
                });
        }
    }
}

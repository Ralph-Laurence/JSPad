ace.define("ace/theme/dark",
["require","exports","module","ace/lib/dom"],
function(e,t,n)
{t.isDark=!0,
    t.cssClass="ace-dark",
    t.cssText=
    `  
    .ace-dark .ace_gutter
    {
        background: #1A1D21;
    }

    .ace-dark .ace_gutter-active-line {
        background-color: #FAC038;
        color: #212121;
    }
    
    .ace-dark {
        color: #CFD0D1;
        background-color: #1F2227;
    }
    
    .ace-dark .ace_invisible {
        color: #504945;
    }
    
    .ace-dark .ace_marker-layer .ace_selection {
        background: rgba(64, 67, 71, 0.75)
    }
    
    .ace-dark.ace_multiselect .ace_selection.ace_start {
        box-shadow: 0 0 3px 0px #002240;
    }
    
    .ace-dark .ace_keyword {
        color: #A558FD;
    }
    
    .ace-dark .ace_comment {
        font-style: italic;
        color: #4E6A87;
    }
    
    .ace-dark .ace-statement {
        color: red;
    }
    
    .ace-dark .ace_variable {
        color: #FDC502;
        background: #2A2A25;
    }  

    .ace-dark .ace_variable.ace_language {
        color: #FDC502;
        background: #2A2A25;
    }
    
    .ace-dark .ace_constant {
        color: #FDC502;
    }
    
    .ace-dark .ace_constant.ace_language {
        color: #FDC502;
    }
    
    .ace-dark .ace_constant.ace_numeric {
        color: #ED6E55;
    }
    
    .ace-dark .ace_string {
        color: #A0BB00;
    }
    
    .ace-gruv .ace_support {
        color: #F9BC41;
    }
    
    .ace-dark .ace_support.ace_function {
        color: #6CE890;
        font-weight: bold;
    }
  
    .ace-dark .ace_storage {
        color: #9D57FD;
        font-weight: bold;
    }

    .ace-dark .ace_paren {
        color: #26AA5A;
        font-weight: bold;
    }
    
    .ace-dark .ace_invalid {
        text-decoration: underline;
    }

    .ace-dark .ace_string.ace_regexp 
    {
        color: #2795EE;
        font-weight: bold;
    }

    .ace-dark .ace_invalid.ace_illegal {
        color: #F8F8F8;
        background-color: rgba(86, 45, 86, 0.75);
    }
    
    .ace-dark .ace_invalid, .ace-dark .ace_deprecated {
        text-decoration: underline;
        font-style: italic;
        color: #D2A8A1;
    }

    .ace-dark .ace_keyword.ace_operator 
    {
        color: #FF3A68;
        font-weight: bold;
    }
    
    .ace-dark .ace_punctuation.ace_operator 
    {
        color: #F3CA63;
    }
    
    .ace-dark .ace_marker-layer .ace_active-line 
    {
        background: #25282D;
    }
    
    .ace-dark .ace_marker-layer .ace_selected-word {
        border-radius: 4px;
        border: 8px solid #3f475d;
    }
    
    .ace-dark .ace_print-margin {
        width: 3px;
        background: #3C3836;
    }
    
    .ace-dark .ace_indent-guide
    {
        background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAACCAYAAACZgbYnAAAAEklEQVQImWMQFxf3ZXB1df0PAAdsAmERTkEHAAAAAElFTkSuQmCC) right repeat-y;
    }
    `;
    var r=e("../lib/dom");r.importCssString(t.cssText,t.cssClass,!1)
});                
(function() 
{
    ace.require(["ace/theme/dark"], function(m) 
    {
        if (typeof module == "object" && typeof exports == "object" && module) 
        {
            module.exports = m;
        }
    });
})();
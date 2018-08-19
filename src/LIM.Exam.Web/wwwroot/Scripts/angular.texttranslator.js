
_app.factory('textTranslator', function () {
    var tl = fetchDefaultTranslator();

    return {
        translateText: function (text) {
            return tl.translateText(text);
        }
    }
});

_app.directive('textTranslator', function ($parse, $interpolate, textTranslator) {

    return {
        restrict: "A",
        transclude: true,
        template: '<ng-transclude></ng-transclude>',
        link: function (scope, element, attrs) {
            element[0].innerHTML = textTranslator.translateText(element.text());
        }
    }
});


//==============================================================
function fetchDefaultTranslator () {
    //default settings
    var _textTranslator = new textTranslator('en-US');
    var data = JSON.parse(sessionStorage.getItem("_savedTranslations_"));

    if (typeof data === 'undefined') {
        data.data.StoredTranslations = [];
        data.data.Culture = 'en-US';
    }

    _textTranslator.storedTranslations = data.StoredTranslations;
    _textTranslator.currentTextCulture = data.Culture

    return _textTranslator;
}

function textTranslator(defaultCulture, currentTextCulture) {

    var _this = this;


    if (defaultCulture == null) {
        defaultCulture = 'en-US';
    }

    _this.storedTranslations = [];
    _this.defaultCulture = defaultCulture;
    _this.currentTextCulture = currentTextCulture;

    _this.stripHTMLFromText = function (text) {

        if ((text.indexOf('<') > -1) && (text.indexOf('>') > -1)) {
            while (text.indexOf('<') > -1) {
                var tagStart = text.IndexOf("<");
                var tagEnd = text.IndexOf(">");
                var tag = text.substring(tagStart, (tagEnd - tagStart));

                text = text.replace(new RegExp('\\b' + tag + '\\b', "ig"), " ");
            }
        }

        return text;
    }

    function translateArrayOfText(arrText) {
        var arrUpdatedText = [];

        $.each(arrText, function (index, text) {
            arrUpdatedText.push(_this.translateText(text));
        })

        return arrUpdatedText;
    }

    _this.translateText = function (text) {

        if (Array.isArray(text)) {
            return translateArrayOfText(text);
        }

        if (_this.currentTextCulture.toLowerCase() == _this.defaultCulture.toLowerCase()) {
            return text;
        }

        var updatedText = $.trim(text);
        updatedText = updatedText.replace('  ', ' ').replace('\r\n', ' ');
        updatedText = _this.stripHTMLFromText(updatedText);

        var arrWords = updatedText.split(/[\s,\/-]+/);

        $.each(arrWords, function (index, word) {
            
            if ($.trim(word).length > 0) {
                word = word.replace(/[^a-z']/gi, '');

                var converted = null;

                //=======================
                $.each(_this.storedTranslations, function (index, translation) {

                    if (translation.k.toLowerCase() == word.toLowerCase()) {
                        converted = translation.v;

                        if (word[0] == word[0].toUpperCase())
                            converted = converted[0].toUpperCase() + converted.substring(1, converted.length);

                        return;
                    } 
                })

                if (converted == null) {

                    //$.post("/Home/GetTranslation",
                    //{
                    //    text: word
                    //},
                    //function (result) {
                    //    if (typeof result === 'object') {
                    //        var arrStoredTranslations = _textTranslator.storedTranslations = data.StoredTranslations;
                    //        arrStoredTranslations.push({ k: result.k, v: result.v });
                    //        converted = result.v;
                    //    }

                    //});
                }
                //=======================
                if (converted == null) {
                    converted = word;
                }
                updatedText = updatedText.replace(new RegExp('\\b' + word + '\\b', "ig"), converted)
            }

        });

        return updatedText;
    }
}


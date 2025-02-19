mergeInto(LibraryManager.library, {
    SetBingoCompleted: function () {
        setBingoStarted();
    },

    SetBingoStarted: function () {
        setBingoCompleted();
    },

    SetOrigin: function (origin) {
        setOrigin(UTF8ToString(origin));
    },

    PostMessage: function (eventType, domen, value, progress = -1) {
        var data = {
            eventType: UTF8ToString(eventType),
            value: UTF8ToString(value),
            progress: progress
        };

        window.parent.postMessage(JSON.stringify(data), UTF8ToString(domen));
    },

    GetUrlParam: function (name) {
        const urlParams = new URLSearchParams(window.location.search);
        const paramValue = urlParams.get(UTF8ToString(name)) || "";
        var bufferSize = lengthBytesUTF8(paramValue) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(paramValue, buffer, bufferSize);
        return buffer;
    },
    
    GetLocalstorageEntry: function (str) {
        var returnStr = window.localStorage.getItem(UTF8ToString(str)) || "";
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },

    SetLocalstorageEntry: function (key, value) {
        window.localStorage.setItem(UTF8ToString(key), UTF8ToString(value));
    },

    GetSessionstorageEntry: function (str) {
        var returnStr = window.sessionStorage.getItem(UTF8ToString(str)) || "";
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },

    SetSessionstorageEntry: function (key, value) {
        window.sessionStorage.setItem(UTF8ToString(key), UTF8ToString(value));
    },

    ShowHTMLPopup: function (popupId) {
        showPopup(UTF8ToString(popupId));
    },

    ShowCreditCardPopup: function (popupId) {
        showPopup("credit-card-popup");
    },

    GetCookie: function (name) {
        var returnStr = getCookie(UTF8ToString(name)) || "";
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },

    OpenWebPage: function (url, target) {
        window.open(UTF8ToString(url), UTF8ToString(target));
    },

    MobileCheck: function () {
        return mobileCheck();
    },

    Logout: function() {
        logout();
    },

    GoToShop: function() {
        goToShop();
    },

    GoToBingo: function() {
        goToBingo();
    },

    GoToChallenges: function() {
        goToChallenges();
    },

    GoToRewards: function() {
        goToRewards();
    },

    GoToMap: function() {
        goToMap();
    },

    GoToLobby: function() {
        goToLobby();
    },

    Refresh: function() {
        goToLobby();
    },

    GrowPet: function(kind) {
        growPet(UTF8ToString(kind));
    },

    GoToGame: function (id) {
        goToGame(UTF8ToString(id));
    },

    GoToDeals: function () {
        goToDeals();
    },

    RevealApp: function() {
        splashScreenHide();
        preloaderHide()
        removePreloadElement();
    },

    SetSound: function (value) {
        setSound(UTF8ToString(value));
    },
         // Example: read a cookie by name, return its value as a pointer to a UTF-8 string in WASM memory
         GetCookieValue: function (namePtr) {
             // Convert the pointer from Unity/C# into a JS string
             var cookieName = UTF8ToString(namePtr);
     
             // Grab the cookie from document.cookie via a RegExp
             var match = document.cookie.match(new RegExp('(^| )' + cookieName + '=([^;]+)'));
             var returnStr = "";
             if (match) {
                 returnStr = match[2];  // The actual cookie value
             }
     
             // Allocate memory in the WASM heap for this string
             var bufferSize = lengthBytesUTF8(returnStr) + 1;
             var buffer = _malloc(bufferSize);
     
             // Copy the JS string into the WASM heap (so C# can read it)
             stringToUTF8(returnStr, buffer, bufferSize);
     
             // Return the pointer (Int32) to the C# side
             return buffer;
         },

    SetMusic: function (value) {
        setMusic(UTF8ToString(value));
    },
        SendMessageToParent: function (message) {
            var data = JSON.parse(UTF8ToString(message));
            window.parent.postMessage(JSON.stringify(data), '*');
        },

});
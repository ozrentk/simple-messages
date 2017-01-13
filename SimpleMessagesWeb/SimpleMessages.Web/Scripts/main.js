require.config({
    baseUrl: "/Scripts",
    paths: {
        jquery: "jquery-3.1.1",
        jqueryValidate: "jquery.validate",
        jqueryValidateUnobtrusive: "jquery.validate.unobtrusive",
        bootstrap: "bootstrap",
        domReady: "domReady",
        jscookie: "js-cookie/js.cookie",
        moment: "moment",
        knockoutjs: "knockout-3.4.1"
    },
    shim: {
        bootstrap: ["jquery"],
        jqueryValidate: ["jquery"],
        jqueryValidateUnobtrusive: ["jquery", "jqueryValidate"]
    }
    // TODO: versioning for production
    //urlArgs: "_=" + (new Date()).getTime()
});

//require(["kickoff"], function (kickoff) {
//    kickoff.init();
//});
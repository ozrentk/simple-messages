require.config({
    baseUrl: "/Scripts",
    paths: {
        jquery: "jquery-3.1.1",
        jqueryValidate: "jquery.validate",
        jqueryValidateUnobtrusive: "jquery.validate.unobtrusive",
        bootstrap: "bootstrap",
        domReady: "domReady",
        jscookie: "js-cookie/js.cookie",
    },
    shim: {
        jqueryValidate: ["jquery"],
        jqueryValidateUnobtrusive: ["jquery", "jqueryValidate"]
    }
});

//require(["kickoff"], function (kickoff) {
//    kickoff.init();
//});
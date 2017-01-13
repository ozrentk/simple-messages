require(["jquery", "jscookie", "bootstrap", "moment", "knockoutjs"], function ($, jscookie, bs, moment, ko) {
    var urlPrefix = 'http://localhost:14221';
    var deliveryIntervalValue = 2000;

    var deliveryInterval = window.setInterval(getDelivery.bind(this), deliveryIntervalValue);

    var aggregateReduce = function (arr, key, val) {
        return arr.reduce(function (pre, cur) {
            var k = cur[key];
            var v = val ? cur[val] : 1;
            pre[k] = (k in pre) ? pre[k] + v : v;
            return pre;
        }, {});
    };

    function getSvcData(uri, callback) {
        var token = jscookie.get(".simplemessages.svctoken");

        $.ajax({
            type: 'GET',
            url: urlPrefix + uri,
            crossDomain: true,
            cache: false,
            headers: { 'x-simplemessages-svctoken': token }
        })
        .done(function (data) {
            if (callback)
                callback(data, true);
        })
        .fail(function (data) {
            if (callback)
                callback(data, false);
        })

    }

    function sendSvcData(uri, sendData, method, callback) {
        var token = jscookie.get(".simplemessages.svctoken");

        $.ajax({
            type: method,
            url: urlPrefix + uri,
            contentType: "application/json",
            data: sendData ? JSON.stringify(sendData) : null,
            crossDomain: true,
            traditional: true,
            cache: false,
            headers: { 'x-simplemessages-svctoken': token }
        })
        .done(function (data) {
            if (callback)
                callback(data, true);
        })
        .fail(function (data) {
            if (callback)
                callback(data, false);
        })

    }

    var messageModel = function (from, to, status, content) {
        this.from = from;
        this.to = to;
        this.status = ko.observable(status);
        this.content = ko.observable(content);
    };

    var conversationModel = function (user, messageCount) {
        this.user = user;
        this.messageCount = ko.observable(messageCount);
    };

    var myViewModel = {
        thisUserToken: ko.observable(''),
        thisUser: ko.observable(chatOptions.currentUser || ''),
        otherUserToken: ko.observable(''),
        otherUser: ko.observable(''),
        conversations: ko.observableArray(),
        activeConversation: ko.observable(),
        messages: ko.observableArray(),

        foundNames: ko.observableArray(),

        thisUserFocus() {
            $('#this-user-select-input').select();
        },
        otherUserFocus() {
            $('#other-user-select-input').select();
        },
        thisUserSearch: function (d, e) {
            if (e.keyCode != 13 && this.thisUserToken() != '') {
                myViewModel.thisUser(null);
                $('#this-user-select-dropdown').toggleClass('open', true);

                getSvcData('/user?search=' + this.thisUserToken(), function (userData, isSuccess) {
                    if (!isSuccess) {
                        console.error("SimpleMessages.Svc user error!");
                        return;
                    }
                    this.foundNames(userData);
                }.bind(this))
            } else if (this.thisUserToken() != '') {
                $('#this-user-select-dropdown').removeClass('open', false);
                var firstFound = '';
                if (this.foundNames().length > 0) {
                    firstFound = this.foundNames()[0];
                    this.thisUser(firstFound);
                } else {
                    this.thisUser(null);
                }
                $('#this-user-select-input').val(firstFound).select();
            } else {
                $('#this-user-select-dropdown').removeClass('open', false);
            }
        },
        otherUserSearch: function (d, e) {
            if (e.keyCode != 13 && this.otherUserToken() != '') {
                myViewModel.otherUser(null);
                $('#other-user-select-dropdown').toggleClass('open', true);

                getSvcData('/user?search=' + this.otherUserToken(), function (userData, isSuccess) {
                    if (!isSuccess) {
                        console.error("SimpleMessages.Svc user error!");
                        return;
                    }
                    this.foundNames(userData);
                }.bind(this))
            } else if (this.otherUserToken() != '') {
                $('#other-user-select-dropdown').removeClass('open', false);
                var firstFound = '';
                if (this.foundNames().length > 0) {
                    firstFound = this.foundNames()[0];
                    this.otherUser(firstFound);
                } else {
                    this.otherUser(null);
                }
                $('#other-user-select-input').val(firstFound).select();
            } else {
                $('#other-user-select-dropdown').removeClass('open', false);
            }
        }
    };

    myViewModel.thisUser.subscribe(function (myViewModel, newValue) {
        console.log("Selected this user " + newValue);
    }.bind(this, myViewModel));

    myViewModel.activeConversation.subscribe(function (myViewModel, newValue) {
        myViewModel.otherUser(newValue);
        myViewModel.otherUserToken(newValue);
        console.log("Selected other user " + newValue);
    }.bind(this, myViewModel));

    ko.applyBindings(myViewModel);

    $("#this-user-select-dropdown ul.dropdown-menu").on('click', 'li > a', function (myViewModel, evt) {
        var found = $(evt.target).text();
        $('#this-user-select-input').val(found);
        myViewModel.thisUser(found);
        myViewModel.thisUserToken(found);
    }.bind(this, myViewModel));

    $("#other-user-select-dropdown ul.dropdown-menu").on('click', 'li > a', function (myViewModel, evt) {
        var found = $(evt.target).text();
        $('#other-user-select-input').val(found);
        myViewModel.otherUser(found);
        myViewModel.otherUserToken(found);
    }.bind(this, myViewModel));

    $('#this-user-select-input').val(chatOptions.currentUser);

    function getDelivery() {
        var thisUser = myViewModel.thisUser();
        if (!thisUser)
            return;

        getSvcData('/delivery?to=' + thisUser, function (data, isSucess) {
            if (!isSucess) {
                console.error("SimpleMessages.Svc delivery error!");
                return;
            }

            console.log('DELIVERY', data);

            var aggCnt = aggregateReduce(data.Messages, 'From');
            for (var user in aggCnt) {
                var existItem = ko.utils.arrayFirst(myViewModel.conversations(), function (item) {
                    return user === item.user;
                });

                if (!existItem) {
                    myViewModel.conversations.push(new conversationModel(user, aggCnt[user]))
                } else if (existItem.messageCount() != aggCnt[user]) {
                    existItem.messageCount(aggCnt[user]);
                }
            }

            for (var i in data.Messages) {
            }

            for (var i in data.Notifications) {
            }
        });
    }
})
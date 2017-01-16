require(["jquery", "jscookie", "bootstrap", "moment", "knockoutjs"], function ($, jscookie, bs, moment, ko) {
    var urlPrefix = 'http://localhost:14221';
    var deliveryIntervalValue = 2000;

    ko.bindingHandlers.toggleClick = {
        init: function (element, valueAccessor) {
            var value = valueAccessor();

            ko.utils.registerEventHandler(element, "click", function () {
                var modelValue = value();
                value(!modelValue);
            });
        }
    };

    var aggregateReduce = function (arr, key, val) {
        return arr.reduce(function (pre, cur) {
            var k = cur[key];
            var v = val ? cur[val] : 1;
            pre[k] = (k in pre) ? pre[k] + v : v;
            return pre;
        }, {});
    };

    var groupReduce = function (arr, key) {
        return arr.reduce(function (pre, cur) {
            var k = cur[key];
            var v = cur;
            if (k in pre)
                pre[k].push(v);
            else
                pre[k] = [v];

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

    var messageModel = function (guid, direction, from, to, status, time, content) {
        this.guid = guid;
        this.direction = direction;
        this.from = from;
        this.to = to;
        this.status = ko.observable(status);
        this.time = time;
        this.timeDisplay = moment(this.time).format('HH:mm:ss');
        this.content = ko.observable(content);
        this.isSelected = ko.observable(false);
        this.statusClass = ko.pureComputed(function () {
            var className;
            switch (this.status()) {
                case 1:
                    return 'status-sent';
                    break;
                case 2:
                    return 'status-received';
                    break;
                case 3:
                    return 'status-read';
                    break;
                default:
                    return 'status-failed';
                    break;
            }
        }, this);
        this.editMessage = function (d, e) {
            console.log("EDIT!");
        }
    };

    var conversationModel = function (user, messages) {
        this.user = user;
        var mapped = messages.map(function (msg) {
            return new messageModel(msg.Guid, msg.Direction, msg.From, msg.To, msg.Status, msg.CreatedAt, msg.Content);
        }.bind(this))
        this.messages = ko.observableArray(mapped);
        this.unreadMessageCount = ko.pureComputed(function () {
            var msgs = $.grep(this.messages(), function (obj, idx) { return obj.status() < 3 });
            return msgs.length;
        }, this)
    };

    var myViewModel = {
        thisUserToken: ko.observable(''),
        thisUser: ko.observable(chatOptions.currentUser || ''),
        otherUserToken: ko.observable(''),
        otherUser: ko.observable(''),
        conversations: ko.observableArray(),
        newMessageContent: ko.observable(''),

        foundNames: ko.observableArray(),

        thisUserFocus() {
            $('#this-user-select-input').select();
        },
        otherUserFocus() {
            $('#other-user-select-input').select();
        },
        thisUserSearch: function (d, e) {
            if (e.keyCode != 13 && !!this.thisUserToken()) {
                myViewModel.thisUser(null);
                $('#this-user-select-dropdown').toggleClass('open', true);

                getSvcData('/user?search=' + this.thisUserToken(), function (userData, isSuccess) {
                    if (!isSuccess) {
                        console.error("SimpleMessages.Svc user error!");
                        return;
                    }
                    this.foundNames(userData);
                }.bind(this))
            } else if (!!this.thisUserToken()) {
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
                myViewModel.thisUser(null);
            }
        },
        otherUserSearch: function (d, e) {
            if (e.keyCode != 13 && !!this.otherUserToken()) {
                //myViewModel.otherUser(null);
                $('#other-user-select-dropdown').toggleClass('open', true);

                getSvcData('/user?search=' + this.otherUserToken(), function (userData, isSuccess) {
                    if (!isSuccess) {
                        console.error("SimpleMessages.Svc user error!");
                        return;
                    }
                    this.foundNames(userData);
                }.bind(this))
            } else if (!!this.otherUserToken()) {
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
                myViewModel.otherUser(null);
                $('#other-user-select-dropdown').removeClass('open', false);
            }
        },
        sendMessage: function (d, e) {
            if (e.keyCode == 13) {
                var content = this.newMessageContent();
                content = $.trim(content);

                // TODO: Send

                // Find active conversation
                var otherUser = this.otherUser();
                var conversations = $.grep(this.conversations(), function (obj, idx) { return obj.user == otherUser });
                if (conversations.length == 0)
                    return;

                // Stuff new message into conversation.messages();
                var thisUser = this.thisUser();
                sendSvcData('/message?from=' + thisUser + '&to=' + otherUser, { Content: content }, 'PUT', function (message, isSuccess) {
                    if (isSuccess) {
                        conversations[0].messages.push(
                            new messageModel(
                                message.Guid,
                                'out',
                                message.From,
                                message.To,
                                message.Status,
                                message.CreatedAt,
                                message.Content))
                    } else {
                        console.error("SimpleMessages.Svc message error!");
                    }
                })

                // Empty message content field
                this.newMessageContent('');
            }
        },
        deleteMessages: function (d, e) {
            var activeConversation = d.activeConversation();
            var messages = activeConversation.messages();
            for (var i = messages.length - 1; i >= 0; i--) {
                if (messages[i].direction == 'in' ||
                    !messages[i].isSelected())
                    continue;

                var deleteGuid = messages[i].guid;
                activeConversation.messages.remove(function (msg) {
                    return msg.guid == deleteGuid;
                });

                sendSvcData('/message?guid=' + deleteGuid, null, 'DELETE', function (empty, isSuccess) {
                    if (isSuccess) {
                        console.log("Message removed");
                    } else {
                        console.error("SimpleMessages.Svc message error!");
                    }
                });
            }
        }
    };

    //myViewModel.thisUser.subscribe(function (myViewModel, newValue) {
    //    console.log("Selected this user " + newValue);
    //}.bind(this, myViewModel));

    myViewModel.otherUser.subscribe(function (myViewModel, newValue) {
        //myViewModel.otherUser(newValue);
        myViewModel.otherUserToken(newValue);

        // Create conversation for user if there is none
        var conversations = $.grep(myViewModel.conversations(), function (obj, idx) { return obj.user == newValue });
        if (conversations.length == 0)
        {
            var convMdl = new conversationModel(newValue, []);
            myViewModel.conversations.push(convMdl);
        }
    }.bind(this, myViewModel));

    myViewModel.bothUsersSelected = ko.pureComputed(function () {
        return !((!this.thisUser()) || (!this.otherUser()));
    }, myViewModel),

    myViewModel.activeConversation = ko.computed(function () {
        var foundConversation = ko.utils.arrayFirst(myViewModel.conversations(), function (item) {
            return item.user == this.otherUser();
        }, this);
        if (!foundConversation)
            return null;

        return foundConversation;
    }, myViewModel);

    myViewModel.activeConversationMessages = ko.computed(function () {
        var foundConversation = ko.utils.arrayFirst(myViewModel.conversations(), function (item) {
            return item.user == this.otherUser();
        }, this);
        if (!foundConversation)
            return null;

        return foundConversation.messages();
    }, myViewModel).extend({ notify: 'always', rateLimit: 200 });

    myViewModel.conversationScrolled = function (d, e) {
        var $conversationBox = $(e.target);
        notifyMessagesRead($conversationBox);
    };

    function notifyMessagesRead($conversationBox) {
        var boxTop = $conversationBox.offset().top;
        var boxBottom = boxTop + $conversationBox.height();

        var $visibles = $conversationBox.find("ul > li").filter(function (idx, el) {
            var $el = $(el);
            var elTop = $el.offset().top;
            var elBottom = elTop + $el.height();
            var test =
                (boxTop < elBottom && elBottom < boxBottom ||
                boxTop < elTop && elTop < boxBottom);

            return test;
        })

        $.each($visibles, function () {
            var msg = ko.dataFor(this);

            // Don't care either about my own messages or those I already read
            if (msg.direction == 'out' || msg.status() == 3)
                return;

            msg.status(3);
            sendSvcData('/notification?guid=' + msg.guid + '&status=3', null, 'PUT', function (message, empty, isSuccess) {
                if (!isSuccess) {
                    console.error("SimpleMessages.Svc notification error!");
                }
            }.bind(this, msg));
        });
    }


    ko.applyBindings(myViewModel);

    // Bootstrap dropdown click handler for "this" user
    $("#this-user-select-dropdown ul.dropdown-menu").on('click', 'li > a', function (myViewModel, evt) {
        var found = $(evt.target).text();
        $('#this-user-select-input').val(found);
        myViewModel.thisUser(found);
        myViewModel.thisUserToken(found);
    }.bind(this, myViewModel));

    // Bootstrap dropdown click handler for "other" user
    $("#other-user-select-dropdown ul.dropdown-menu").on('click', 'li > a', function (myViewModel, evt) {
        var found = $(evt.target).text();
        $('#other-user-select-input').val(found);
        myViewModel.otherUser(found);
        myViewModel.otherUserToken(found);
    }.bind(this, myViewModel));

    // Textarea stretching handler for the new message
    $("#new-message-content").css({
        'overflow-y': 'hidden'
    }).on('input', function () {
        this.style.height = 'auto';
        this.style.height = (this.scrollHeight) + 'px';
    });

    // Setting the current user textfield
    // TODO: is there a smarter way?
    $('#this-user-select-input').val(chatOptions.currentUser);

    // Main delivery loop
    // Process new messages and notifications
    var deliveryInterval = window.setInterval(getDelivery.bind(this), deliveryIntervalValue);
    function getDelivery() {
        var thisUser = myViewModel.thisUser();
        if (!thisUser)
            return;

        getSvcData('/delivery?to=' + thisUser, function (data, isSucess) {
            if (!isSucess) {
                console.error("SimpleMessages.Svc delivery error!");
                return;
            }

            // Exit if nothing received
            if (data.Messages.length == 0 && data.Notifications.length == 0)
                return;

            // TODO: remove this
            console.log('NEW NONEMPTY DELIVERY', data);

            // Group messages by 'From' (other) users
            var fromGroups = groupReduce(data.Messages, 'From');
            for (var user in fromGroups) {
                // Get conversation for other user
                var usersConversation = ko.utils.arrayFirst(myViewModel.conversations(), function (item) {
                    return user == item.user;
                });

                // Either add new conversation and messages to it or
                // insert/update messages to an existing conversation
                if (!usersConversation) {
                    var messages = fromGroups[user];
                    // Patch 'direction' attr
                    $.each(messages, function () { this.Direction = 'in'; });
                    var convMdl = new conversationModel(user, messages);
                    myViewModel.conversations.push(convMdl);
                } else {
                    // TODO: compare this vs classic O(n2) vs divide-and-conquer a cached-and-sorted-array
                    $.each(fromGroups[user], function () {
                        var matchMsgs = $.grep(usersConversation.messages(), function (msg) {
                            return msg.guid == this.Guid;
                        }.bind(this))

                        var matchMsg = matchMsgs[0];

                        if (matchMsgs.length > 0) {
                            // Update message
                            if (matchMsg.content() != this.Content)
                                matchMsg.content(this.Content);

                            if (matchMsg.status() < this.Status) // Only newer statuses
                                matchMsg.status(this.Status);
                        } else {
                            // Insert message
                            usersConversation.messages.push(new messageModel(
                                this.Guid,
                                'in',
                                this.To,
                                this.From,
                                this.Status,
                                this.CreatedAt,
                                this.Content));
                        }
                    });
                }

                // Confirm (notify) each message as received
                $.each(fromGroups[user], function () {
                    sendSvcData('/notification?guid=' + this.Guid + '&status=2', null, 'PUT', function (empty, isSuccess) {
                        if (!isSuccess) {
                            console.error("SimpleMessages.Svc notification error!");
                        }
                    }.bind(this));
                });
            }

            // Group notifications by 'Origin' (other) users
            var originGroups = groupReduce(data.Notifications, 'OriginUser');
            for (var user in originGroups) {
                // Get conversation for origin-user
                var usersConversation = ko.utils.arrayFirst(myViewModel.conversations(), function (item) {
                    return user == item.user;
                });

                // Either add new conversation and messages to it or
                // insert/update messages to an existing conversation
                if (!usersConversation)
                    return; // Never mind the notification...

                // TODO: compare this vs classic O(n2) vs divide-and-conquer a cached-and-sorted-array
                $.each(originGroups[user], function () {
                    var matchMsgs = $.grep(usersConversation.messages(), function (msg) {
                        return msg.guid == this.MessageGuid;
                    }.bind(this))

                    var matchMsg = matchMsgs[0];

                    if (this.NotificationType == 1 && matchMsgs.length > 0) {
                        // Update message status
                        if (matchMsg.status() < this.MessageStatus) // Only newer statuses
                            matchMsg.status(this.MessageStatus);
                    } else if (this.NotificationType == 2 && matchMsgs.length > 0) {
                        // Update message content
                        if (matchMsg.content() != this.Content)
                            matchMsg.content(this.Content);
                    } else if (this.NotificationType == 3 && matchMsgs.length > 0) {
                        // Delete message
                        usersConversation.messages.remove(matchMsg);
                    }
                });
            }
        });

        // Aditionally - check if there are messages read, and notify that
        var $conversationBox = $("#conversation-box");
        notifyMessagesRead($conversationBox);
    }
})
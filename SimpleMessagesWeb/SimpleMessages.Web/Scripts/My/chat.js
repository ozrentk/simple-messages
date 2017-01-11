require(["jquery", "jscookie", "bootstrap", "moment"], function ($, jscookie, bs, moment) {
    this.urlPrefix = 'http://localhost:14221';
    this.viewState = {
        userSelecting: false,
        messageSelecting: false,
        messageEditing: false
    };
    this.dialogs = {};
    this.$thisUser = $('#this-user');
    this.$otherUser = $('#other-user');
    this.$conversationArea = $('#conversation-area');
    this.$newMessageContent = $('#new-message-content');

    function getStatusClassName(status) {
        switch (status) {
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
    }

    function getSvcData(uri, callback) {
        var token = jscookie.get(".simplemessages.svctoken");

        $.ajax({
            type: 'GET',
            url: this.urlPrefix + uri,
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
            url: this.urlPrefix + uri,
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

    function jumpToEnd() {
        // TODO: MS Edge has problems with this
        var h = this.$conversationArea.scrollTop() + this.$conversationArea.height();
        this.$conversationArea.scrollTop(h);
    }

    function getDelivery() {
        var thisUser = this.$thisUser.val();
        getSvcData('/delivery?to=' + thisUser, function (data, isSucess) {
            if (!isSucess)
            {
                console.error("SimpleMessages.Svc delivery error!");
                return;
            }

            for (var i in data.Messages) {
                var message = data.Messages[i];
                ensureOtherUser(message.From);
                addMesage('in', message.From, message.Guid, message.Content, message.Status, message.CreatedAt);
                sendSvcData('/notification?guid=' + message.Guid + '&status=2', null, 'PUT', function (message, empty, isSuccess) {
                    if (isSuccess) {
                        setMessageStatus('out', message.From, message.Guid, 2);
                    } else {
                        console.error("SimpleMessages.Svc notification error!");
                    }
                }.bind(this, message));
            }
            if (data.Messages.length > 0) {
                jumpToEnd();
            }
            for (var i in data.Notifications) {
                var notification = data.Notifications[i];
                ensureOtherUser(notification.OriginUser);
                processNotification(notification.OriginUser, notification.NotificationType, notification.MessageGuid, notification.MessageStatus, notification.MessageContent);
            }

            for (var otherUser in this.dialogs) {
                var tmpMsg = $.grep(this.dialogs[otherUser].messages, function (obj, idx) {
                    return (obj.Direction == 'in' && obj.Status == 2 ? true : false);
                });

                $badge = this.dialogs[otherUser].$el.find('span.badge');
                if (tmpMsg.length == 0) {
                    $badge.text("");
                } else if (+$badge.text() != tmpMsg.length) {
                    $badge.text(tmpMsg.length);
                }
            }
        });
    }

    function ensureOtherUser(otherUser) {
        var $el;
        if (otherUser in this.dialogs) {
            $el = this.dialogs[otherUser].$el;
        } else {
            var $newContainer = $('<ul>', { 'class': 'list-unstyled list-blocks' });
            $el = $('<a href="#" data-user="' + otherUser + '" class="list-group-item active">' + otherUser + '<span class="badge"></span></a>');
            this.dialogs[otherUser] = { messages: [], $container: $newContainer, $el: $el };

            var $otherUserList = $('#other-user-list');
            $otherUserList.append($el);
        }

        return $el;
    }

    function activateConversation() {
        var thisUser = this.$thisUser.val();
        var otherUser = this.$otherUser.val();

        this.$newMessageContent.prop('disabled', false);

        var oldContainers = this.$conversationArea.children();
        oldContainers.remove();
        this.$conversationArea.append($(this.dialogs[otherUser].$container));

        window.setTimeout(function () {
            jumpToEnd();
            processUnreadMessages();
        }, 1);
    }

    function addMesage(direction, otherUser, messageGuid, content, status, time) {
        var $el = addViewMessage(this.dialogs[otherUser].$container, direction, otherUser, messageGuid, content, status, time);

        this.dialogs[otherUser].messages.push({
            Direction: direction,
            Guid: messageGuid,
            Content: content,
            Status: status,
            Time: time,
            Selected: false,
            $el: $el
        });
    }

    function addViewMessage($container, direction, otherUser, messageGuid, content, status, timeStr) {
        //var time = toJavaScriptDate(timeStr);
        var timeStr2 = moment(timeStr).format('hh:mm:ss');

        var statusClass;
        if (direction == 'out') {
            statusClass = getStatusClassName(status);
        } else {
            statusClass = 'hidden';
        }

        // TODO: move this to template
        // TODO: instead of UTC show local time
        var html =
            '<li class="' + (direction == 'in' ? '' : 'text-right') + '" data-dir="' + direction + '" data-msg-id="' + messageGuid + '">' +
                '<div class="msg-e msg-e0 noselect">' +
                    '<span class="glyphicon glyphicon-remove"></span>' +
                    '<span class="glyphicon glyphicon-pencil"></span>' +
                '</div>' +
                '<div class="panel panel-default msg-pnl">' +
                    '<div class="panel-body">' +
                        '<div class="mcg-cnt">' + content + '</div>' +
                        '<small><span class="msg-dt">' + timeStr2 + ' UTC</span>&nbsp;<span class="glyphicon glyphicon-ok ' + statusClass + '"></span></small>' +
                    '</div>' +
                '</div>' +
            '</li>';

        var $el = $(html);
        $container.append($el);

        return $el;
    }

    function processNotification(originUser, notificationType, messageGuid, messageStatus, messageContent) {
        if (notificationType == 1) {
            setMessageStatus('in', originUser, messageGuid, messageStatus);
        }
        else if (notificationType == 2) {
            updateMessageContent(originUser, messageGuid, messageContent);
        }
        if (notificationType == 3) {
            removeMessage(originUser, messageGuid);
        }
    }

    function setMessageStatus(direction, originUser, messageGuid, status) {
        var msg = $.grep(this.dialogs[originUser].messages, function (obj, idx) {
            return obj.Guid == messageGuid;
        });
        if (msg.length == 0)
            return;

        msg[0].Status = status;

        if (direction == 'in')
            setViewMessageStatus(msg[0].$el, status);
    }

    function removeMessage(originUser, messageGuid) {
        var delIdx;
        var msg = $.grep(this.dialogs[originUser].messages, function (obj, idx) {
            delIdx = idx;
            return obj.Guid == messageGuid;
        });
        if (msg.length == 0)
            return;

        // Remove message
        this.dialogs[originUser].messages.splice(delIdx, 1);

        removeViewMessage(msg[0].$el);
    }

    function removeViewMessage($el) {
        $el.remove();
    }

    function updateMessageContent(originUser, messageGuid, content) {
        var msg = $.grep(this.dialogs[originUser].messages, function (obj, idx) {
            return obj.Guid == messageGuid;
        });
        if (msg.length == 0)
            return;

        msg[0].Content = content;
        updateViewMessageContent(msg[0].$el, content);
    }

    function updateViewMessageContent($el, content) {
        $el.find('.mcg-cnt').text(content);
    }

    function setViewMessageStatus($el, status) {
        var $statusIcon = $el.find('span.glyphicon');
        $statusIcon[0].className = $statusIcon[0].className.replace(/\b(status\-.+)?\b/g, '');
        var statusClassName = getStatusClassName(status);
        $statusIcon.addClass(statusClassName);
    }

    function selectViewMessage($el, mustSelect) {
        $el.toggleClass('msg-sel', mustSelect);
        $el.find('div.msg-e').toggleClass('msg-e0', !mustSelect);
    }

    function selectMessage($el, direction, thisUser, otherUser, messageGuid) {
        // Incoming messages not supported for edit/delete
        if (direction == 'in')
            return;

        var msgs = $.grep(this.dialogs[otherUser].messages, function (obj, idx) {
            return obj.Guid == messageGuid;
        });
        var msg = msgs[0];

        var mustSelect = !msg.Selected;

        msg.Selected = mustSelect;
        selectViewMessage($el, mustSelect)

        // Toggle select mode
        var selectedMessages = $.grep(self.dialogs[otherUser].messages, function (obj, idx) {
            return obj.Selected;
        })
        self.viewState.messageSelecting = (selectedMessages.length > 0);

    }

    function processUnreadMessages() {
        var messages = getVisibleUnreadMessages();
        var from = this.$otherUser.val();

        for (var i = 0; i < messages.length; i++) {
            var message = messages[i];
            sendSvcData('/notification?guid=' + message.Guid + '&status=3', null, 'PUT', function (message, empty, isSuccess) {
                if (isSuccess) {
                    setMessageStatus('in', from, message.Guid, 3);
                } else {
                    console.error("SimpleMessages.Svc notification error!");
                }
            }.bind(this, message));
        }
    }

    function getVisibleUnreadMessages() {
        var areaTop = this.$conversationArea.offset().top;
        var areaBottom = areaTop + this.$conversationArea.height();

        var originUser = this.$otherUser.val();
        var visUnrdMessages = $.grep(this.dialogs[originUser].messages, function (obj, idx) {
            if (obj.Direction == 'out' || obj.Status == 3)
                return false;

            var elTop = obj.$el.offset().top;
            var elBottom = elTop + obj.$el.height();

            return (
                areaTop < elBottom && elBottom < areaBottom ||
                areaTop < elTop && elTop < areaBottom);
        });

        return visUnrdMessages;
    };

    function sendNewMessage() {
        var thisUser = this.$thisUser.val();
        var otherUser = this.$otherUser.val();
        var content = this.$newMessageContent.val();

        // Clean up content before sending
        var cleanContent = content.replace(/\n/g, "");

        sendSvcData('/message?from=' + thisUser + '&to=' + otherUser, { Content: cleanContent }, 'PUT', function (message, isSuccess) {
            if (isSuccess) {
                // TODO: remove special case (čćđ, newline - need base64 decode)
                addMesage('out', message.To, message.Guid, message.Content, message.Status, message.CreatedAt);
                jumpToEnd();
            } else {
                console.error("SimpleMessages.Svc message error!");
            }
        })

        // workaround to avoid leaving one newline in the textarea
        window.setTimeout(function () {
            this.$newMessageContent.val("");
        }.bind(this), 1);

    }

    function handleEditedMessage(otherUser, messageGuid, content) {
        var newContent = this.$newMessageContent.val();
        var newCleanContent = newContent.replace(/\n/g, "");

        // workaround to avoid leaving one newline in the textarea
        window.setTimeout(function () {
            this.$newMessageContent.val("");
        }.bind(this), 1);

        if (content == newCleanContent)
            return;
        
        var self = this;
        sendSvcData('/message?guid=' + messageGuid, { Content: newCleanContent }, 'POST', function (otherUser, messageGuid, content, empty, isSuccess) {
            if (isSuccess) {
                updateMessageContent(otherUser, messageGuid, content);
            } else {
                console.error("SimpleMessages.Svc message error!");
            }

            // reattach
            self.$newMessageContent.off('keypress');
            self.$newMessageContent.off('blur');
            self.$newMessageContent.on('keypress', function (e) {
                if (e.which == 13) {
                    sendNewMessage();
                }
            });
        }.bind(this, otherUser, messageGuid, newCleanContent));

        $('#screen-overlay').fadeOut(150, function () {
            $('.focus-overlay').css('z-index', '1');
        });

    }

    $(function () {
        var $thisUserChange = $('#this-user-change');
        var $thisUserSelect = $('#this-user-select');
        var $otherUserChange = $('#other-user-change');
        var $otherUserSelect = $('#other-user-select');
        var $otherUserList = $('#other-user-list');

        var self = this;

        this.$newMessageContent.on('keypress', function (e) {
            if (e.which == 13) {
                sendNewMessage();
            }
        });

        this.$conversationArea.scroll(function () {
            processUnreadMessages();
        });

        this.$newMessageContent.css({
            'height': this.$newMessageContent + 'px',
            'overflow-y': 'hidden'
        }).on('input', function () {
            this.style.height = 'auto';
            this.style.height = (this.scrollHeight) + 'px';
        });

        var deliveryInterval = window.setInterval(getDelivery.bind(this), 2000);

        /* BEGIN: SELECT THIS USER */

        $thisUserChange.on('click', function (e) {
            self.$thisUser.prop('readonly', false);
            self.$thisUser.focus();

            if (self.$thisUser.val()) {
                self.$thisUser.val("");
                $thisUserSelect.empty();
                $thisUserSelect.css('display', 'block');
            }
        })

        $thisUserSelect.on('click', 'button', function (e) {
            var user = $(this).text();
            self.$thisUser.val(user);
            self.$thisUser.prop('readonly', true);
            $thisUserSelect.css('display', 'none');
        })

        $conversationArea.on('click', 'ul li', function (e) {
            var thisUser = self.$thisUser.val();
            var otherUser = self.$otherUser.val();
            var messageGuid = $(this).data('msg-id');
            var direction = $(this).data('dir');
            selectMessage($(this), direction, thisUser, otherUser, messageGuid);
        })

        $conversationArea.on('click', 'ul li div.msg-e span', function (e) {
            if ($(this).hasClass('glyphicon-remove')) {
                var messageGuid = $(this).closest('li').data('msg-id');
                $('#confirm-deletion-dlg').data('msg-id', messageGuid).modal('show');
            } else if ($(this).hasClass('glyphicon-pencil')) {
                var messageGuid = $(this).closest('li').data('msg-id');
                var otherUser = self.$otherUser.val();
                var msg = $.grep(self.dialogs[otherUser].messages, function (obj, idx) {
                    return obj.Guid == messageGuid;
                });

                var content = msg[0].Content;
                self.$newMessageContent.val(content);
                self.$newMessageContent.css('z-index', '100');
                $('#screen-overlay').fadeIn(150);
                self.$newMessageContent.focus();
                // detach
                self.$newMessageContent.off('keypress');
                self.$newMessageContent.on('keypress', function (e) {
                    if (e.which == 13) {
                        handleEditedMessage(otherUser, messageGuid, content);
                    }
                });
                self.$newMessageContent.on('blur', function () {
                    handleEditedMessage(otherUser, messageGuid, content);
                });
            }
        })

        $('#screen-overlay').click(function (e) {
            $('#screen-overlay').fadeOut(150, function () {
                $('.focus-overlay').css('z-index', '1');
            });
        });

        $('#confirm-deletion-dlg .modal-footer button').on('click', function (e) {
            if ($(this).text() == 'Ok') {
                $('#confirm-deletion-dlg').data('result', true);
            } else {
                $('#confirm-deletion-dlg').data('result', false);
            }
        });

        $('#confirm-deletion-dlg').on('hide.bs.modal', function (e) {
            var result = $(this).data('result');
            if (result) {
                var otherUser = self.$otherUser.val();
                var messageGuid = $(this).data('msg-id');
                sendSvcData('/message?guid=' + messageGuid, null, 'DELETE', function (otherUser, messageGuid, empty, isSuccess) {
                    if (isSuccess) {
                        removeMessage(otherUser, messageGuid);
                    } else {
                        console.error("SimpleMessages.Svc message error!");
                    }
                }.bind(this, otherUser, messageGuid));

            }
        });

        // 'keyup' detects backspace
        // TODO: add setTimeout for search optimization
        this.$thisUser.on('keyup', function (e) {
            if (e.which != 13) {
                $thisUserSelect.css('display', 'block');

                var search = self.$thisUser.val();
                getSvcData('/user?search=' + search, function (userData, isSuccess) {
                    if (!isSucess) {
                        console.error("SimpleMessages.Svc user error!");
                        return;
                    }

                    userData = userData.slice(0, 3);
                    var $buttonList = $.map(userData, function (item) {
                        return $('<button>', { text: item, 'class': 'btn btn-default' });
                    })
                    $thisUserSelect.empty().append($buttonList);
                })
            } else {
                self.$thisUser.prop('readonly', true);
                $thisUserSelect.css('display', 'none');
            }
        })
        /* END: SELECT THIS USER */

        /* BEGIN: SELECT OTHER USER */
        $otherUserChange.on('click', function (e) {
            self.$otherUser.prop('readonly', false);
            self.$otherUser.focus();

            if (self.$otherUser.val()) {
                self.$otherUser.val("");
                $otherUserSelect.empty();
                $otherUserSelect.css('display', 'block');
            }
        })

        $otherUserSelect.on('click', 'button', function (e) {
            var otherUser = $(this).text();
            self.$otherUser.val(otherUser);
            self.$otherUser.prop('readonly', true);
            $otherUserSelect.css('display', 'none');

            $('#other-user-list a').removeClass('active');

            var $otherUserItem = ensureOtherUser(otherUser);
            $otherUserItem.addClass('active');

            activateConversation();
        })

        $otherUserList.on('click', 'a', function (e) {
            var otherUser = $(this).data('user');
            self.$otherUser.val(otherUser);

            $('#other-user-list a').removeClass('active');
            $(this).addClass('active');

            activateConversation();
        })

        // 'keyup' detects backspace
        // TODO: add setTimeout for search optimization
        this.$otherUser.on('keyup', function (e) {
            if (e.which != 13) {
                $otherUserSelect.css('display', 'block');

                var search = self.$otherUser.val();
                getSvcData('/user?search=' + search, function (userData, isSucess) {
                    if (!isSucess) {
                        console.error("SimpleMessages.Svc user error!");
                        return;
                    }

                    userData = userData.slice(0, 3);
                    var $buttonList = $.map(userData, function (item) {
                        return $('<button>', { text: item, 'class': 'btn btn-default' });
                    })
                    $otherUserSelect.empty().append($buttonList);
                })
            } else {
                self.$otherUser.prop('readonly', true);
                $otherUserSelect.css('display', 'none');
            }
        })
        /* END: SELECT OTHER USER */

    }.bind(this));
});
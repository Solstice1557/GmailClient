$(function () {
    'use strict';

    function viewModel() {
        var self = this;

        self.account = ko.observable("");
        self.accountLoading = ko.observable(true);
        self.accountPasswordToSave = ko.observable("");
        self.accountToSave = ko.observable("");
        self.accountToSaveIsNotCorrect = ko.computed(function() {
            var acc = this.accountToSave();
            if (!acc) {
                return false;
            }

            return !validateEmail(acc);
        }, self);
        self.canSaveAccount = ko.computed(function () {
            var acc = this.accountToSave();
            var pass = this.accountPasswordToSave();
            return !!acc && !!pass && validateEmail(acc);
        }, self);
        self.saveAccount = function() {
            var acc = self.accountToSave();
            var pass = self.accountPasswordToSave();
            if (!acc || !pass) {
                showAlert("Enter gmail account and password.");
                return;
            }

            if (!validateEmail(acc)) {
                showAlert("Please enter valid email.");
                return;
            }

            var requestParams = {
                UserName: acc,
                Password: pass,
            };

            self.accountLoading(true);
            $.post("/Account/SetGmailAccount", requestParams, function (data) {
                    if (data) {
                        if (data.Success) {
                            self.account(self.accountToSave());
                            self.accountPasswordToSave("");
                            self.accountToSave("");
                        } else {
                            showAlert(data.Message);
                        }
                    }
                })
                .fail(function() {
                    showAlert("Error while requesting server!");
                })
                .always(function() {
                    self.accountLoading(false);
                });
        };
        self.removeAccount = function() {
            if (confirm("Are you shure you want to remove account?")) {
                self.accountLoading(true);
                $.post("/Account/ResetGmailAccount", {}, function (data) {
                    if (data) {
                        if (data.Success) {
                            self.account("");
                        } else {
                            showAlert(data.Message);
                        }
                    }
                })
                .fail(function () {
                    showAlert("Error while requesting server!");
                })
                .always(function () {
                    self.accountLoading(false);
                });
            }
        };
        self.account.subscribe(function (newValue) {
            if (newValue) {
                self.refreshMail();
            }
        });

        self.mailLoading = ko.observable(false);
        self.currentPage = ko.observable(1);
        self.currentPageSize = ko.observable("10");
        self.totalLetters = ko.observable(0);
        self.lettersOnPage = ko.observableArray([]);
        self.maxPageNumber = ko.computed(function() {
            var total = this.totalLetters();
            var pageSize = this.currentPageSize();
            return Math.ceil(total / pageSize);
        }, self);
        self.refreshMail = function() {
            if (!self.account()) {
                return;
            }

            loadPage(1);
        };
        self.currentPageSize.subscribe(function() {
            self.refreshMail();
        });

        self.nextPage = function() {
            var currentPage = self.currentPage();
            var maxPage = self.maxPageNumber();
            if (currentPage >= maxPage) {
                return;
            }

            loadPage(currentPage + 1);
        };
        self.prevPage = function () {
            var currentPage = self.currentPage();
            if (currentPage <= 1) {
                return;
            }

            loadPage(currentPage - 1);
        };
        self.lastPage = function () {
            var currentPage = self.currentPage();
            var maxPage = self.maxPageNumber();
            if (currentPage >= maxPage) {
                return;
            }

            loadPage(maxPage);
        };
        self.firstPage = function () {
            var currentPage = self.currentPage();
            var maxPage = self.maxPageNumber();
            if (currentPage >= maxPage) {
                return;
            }

            loadPage(1);
        };

        self.currentMail = ko.observable(null);
        self.backToList = function() {
            self.currentMail(null);
            self.newMail(null);
        };
        self.loadMail = function (mail) {
            if (mail.loaded) {
                self.currentMail(mail);
            } else {
                self.mailLoading(true);
                $.getJSON("/api/Mail/Get/" + mail.Uid, {}, function(response) {
                        if (response.Success) {
                            var result = response.Data;
                            mail.Body = result.Body;
                            mail.Attachments = result.Attachments;
                            mail.loaded = true;
                            self.currentMail(mail);
                        } else {
                            showAlert(response.Message);
                        }

                    })
                    .fail(function() {
                        showAlert("Error while requesting server!");
                    })
                    .always(function() {
                        self.mailLoading(false);
                    });
            }
        };
        self.deleteMail = function (mail) {
            if (!mail) {
                return;
            }

            self.mailLoading(true);
            $.getJSON("/api/Mail/Delete/" + mail.Uid, {}, function(data) {
                    if (data.Success) {
                        self.currentMail(null);
                        self.lettersOnPage.remove(function(item) { return item.Uid === mail.Uid; });
                    } else {
                        showAlert(data.Message);
                    }
                }).fail(function() {
                    showAlert("Error while requesting server!");
                })
                .always(function() {
                    self.mailLoading(false);
                });
        };

        self.newMail = ko.observable(null);
        self.replyMail = function(mail) {
            self.currentMail(null);
            self.newMail(createNewMail(mail.From, mail.Subject));
        };
        self.createMail = function() {
            self.currentMail(null);
            self.newMail(createNewMail("", ""));
        };
        self.sendMail = function() {
            var mail = self.newMail();
            if (!mail) {
                return;
            }

            var params = ko.toJS(mail);
            if (!validateEmail(params.to)) {
                showAlert("Please enter valid email.");
                return;
            }

            if (!params.subject) {
                showAlert("Please enter subject.");
                return;
            }

            if (!params.body) {
                showAlert("Please enter email text.");
                return;
            }

            self.mailLoading(true);
            $.getJSON("/api/Mail/Send", params, function(data) {
                    if (data.Success) {
                        self.newMail(null);
                        showAlert("Mail sended");
                    } else {
                        showAlert(data.Message);
                    }
                }).fail(function() {
                    showAlert("Error while requesting server!");
                })
                .always(function() {
                    self.mailLoading(false);
                });
        };

        function createNewMail(to, subject) {
            if (subject) {
                subject = "RE: " + subject;
            }

            return {
                to: ko.observable(to),
                subject: ko.observable(subject),
                body: ko.observable("")
            };
        }

        function loadPage(page) {
            var params = {};
            if (page) {
                params.page = page;
            }

            var count = self.currentPageSize();

            if (count) {
                params.count = count;
            }

            self.mailLoading(true);
            $.getJSON("/api/Mail/Get", params,
                    function (response) {
                        if (response.Success) {
                            var result = response.Data;
                            self.lettersOnPage.removeAll();
                            self.lettersOnPage(result.Mails);
                            self.totalLetters(result.Total);
                            self.currentPage(result.Page);
                            self.currentMail(null);
                            self.newMail(null);
                        } else {
                            showAlert(response.Message);
                        }
                    })
                .fail(function() {
                    showAlert("Error while requesting server!");
                })
                .always(function() {
                    self.mailLoading(false);
                });
        }

        $.get("/Account/GetGmailAccount", {}, function(data) {
                self.account(data);
            })
            .fail(function() {
                showAlert("Error while requesting server!");
            })
            .always(function() {
                self.accountLoading(false);
            });
    };

    function showAlert(msg) {
        alert(msg);
    }

    function validateEmail(email) {
        var re = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
        return re.test(email);
    }

    var model = new viewModel();
    ko.applyBindings(model);
});
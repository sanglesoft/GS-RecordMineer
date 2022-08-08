$("form").on("submit", function (event) {
    event.preventDefault();
});
var dataTable = [];
var myAudioFile = "";
var myAudioState = "";
var myAudio = new Audio();

function fillFormNAS() {
    $.ajax({
        type: "POST",
        url: "/getNASConfig/",
        success: function (result) {
            console.log(result);
            if (result.statusCode == 200 && result.data.length>0) {
                $("#form-connect-host").val(result.data[0].host);
                $("#form-connect-sharename").val(result.data[0].sharename);
                $("#form-connect-username").val(result.data[0].username);
                $("#form-connect-password").val(result.data[0].password);
            }
            else {
                $.toast({
                    heading: "[Error " + (result.statusCode / 10) + "x] get data failed",
                    text: result.message + (result.data != "" ? ("<br>" + JSON.stringify(result.data)) : ""),
                    icon: 'error',
                    showHideTransition: 'slide',
                    position: 'bottom-right',
                });
            }
        },
        error: function (result) {
            console.log(result);
            $.toast({
                heading: "Error",
                text: JSON.stringify(result),
                icon: 'error',
                showHideTransition: 'slide',
                position: 'bottom-right',
            });
        },
        complete: function () {

        }
    });
}

function filltableFilter() {
    $.ajax({
        type: "POST",
        url: "/getFromNAS/",
        success: function (result) {
                console.log(result);
            if (result.statusCode == 200) {
                $("#tableFilter tbody").html("");
                result.data.forEach(function (item) {
                    var filename = item.split('\\').pop();
                    dataTable.push({
                        from: filename.split('-')[2],
                        to: filename.split('-')[3].split('.')[0],
                        time: moment(filename.split('-')[1], 'X').format('lll'),
                        path: item
                    })
                });
                dataTable.sort((a, b) => a.value - b.time);
                dataTable.reverse();
                dataTable.forEach(function (item) {
                    $("#table-filter tbody").append("<tr><td>" + item.from + "</td><td>" + item.to + "</td><td>" + item.time + "</td><td class=\"cell-durration\"></td><td><a href=\"#\" class=\"art-link btnPlay\" data-id=\"" + toHex(item.path) + "\"><i class=\"fa fa-2x fa-volume-high\" data-id=\"" + toHex(item.path) + "\"></i></a></td></tr>");
                });
            }
            else {
                $.toast({
                    heading: "[Error " + (result.statusCode / 10) + "x] get data failed",
                    text: result.message + (result.data != "" ? ("<br>" + JSON.stringify(result.data)) : ""),
                    icon: 'error',
                    showHideTransition: 'slide',
                    position: 'bottom-right',
                });
            }
        },
        error: function (result) {
            console.log(result);
            $.toast({
                heading: "Error",
                text: JSON.stringify(result),
                icon: 'error',
                showHideTransition: 'slide',
                position: 'bottom-right',
            });
        },
        complete: function () {

        }
    });
}
function toHex(str) {
    var result = '';
    for (var i = 0; i < str.length; i++) {
        result += str.charCodeAt(i).toString(16);
    }
    return result;
}
$(document).ready(function () {
    fillFormNAS();
    filltableFilter();
    $.ajax({
        type: "POST",
        url: "/isLogin",
        success: function (result) {
            if (!result) {
                $('#login-modal').modal({ backdrop: 'static', keyboard: false });  
            }
            else {
                $('#login-modal').modal('hide');
            }
        },
        error: function (result) {
            
        },
        complete: function () {
        }
    });
        
    $("#login-modal").on('hidden.bs.modal', function (e) {
        $.ajax({
            type: "POST",
            url: "/isLogin",
            success: function (result) {
                if (!result) {
                    $("#login-modal").modal('show');
                }
            },
            error: function (result) {

            },
            complete: function () {
            }
        });
    });


    const root = document.querySelector('.timeRange');
    const txtStart = root.querySelector('#Start');
    const txtEnd = root.querySelector('#End');
    const container = root.querySelector('.ex-inputs-picker');
    DateRangePicker.DateRangePicker(container)
        .on('statechange', function (_, rp) {
            var range = rp.state;
            txtStart.value = range.start ? moment(range.start).format('DD/MM/YYYY') : '';
            txtEnd.value = range.end ? moment(range.end).format('DD/MM/YYYY') : '';
        });
    txtStart.addEventListener('focus', showPicker);
    txtEnd.addEventListener('focus', showPicker);
    function showPicker() {
        container.classList.add('ex-inputs-picker-visible');
    }
    let previousTimeout;
    root.addEventListener('focusout', function hidePicker() {
        clearTimeout(previousTimeout);
        previousTimeout = setTimeout(function () {
            if (!root.contains(document.activeElement)) {
                container.classList.remove('ex-inputs-picker-visible');
            }
        }, 10);
    });

    setInputFilter(document.getElementById("From"), function (value) {
        return /^\d*\.?\d*$/.test(value); 
    }, "");
    setInputFilter(document.getElementById("To"), function (value) {
        return /^\d*\.?\d*$/.test(value); 
    }, "");
    setInputFilter(document.getElementById("Start"), function (value) {
        return /^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[13-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{2})$/.test(value); 
    }, "");
    setInputFilter(document.getElementById("End"), function (value) {
        return /^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[13-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{2})$/.test(value); 
    }, "");
    
    $("#btn-submit-login").on("click", function () {
        $("#btn-submit-login").prop("disabled", true);
        $.ajax({
            type: "POST",
            url: "/verifyLogin/",
            data: {
                "Alias": $("#form-login-alias").val(),
                "Password": $("#form-login-password").val()
            },
            success: function (result) {
                if (result.statusCode == 200) {
                    
                    $.toast({
                        heading: "Login Success with " + result.data,
                        text: "",
                        icon: 'success',
                        showHideTransition: 'slide',
                        position: 'bottom-right',
                        afterHidden: function () {
                            
                        }
                    });
                    $.ajax({
                        type: "POST",
                        url: "/isLogin/",
                        success: function (result) {
                            if (result) {
                                $("#login-modal").modal('hide');
                            }
                            else {
                                $("#login-modal").modal('show');
                            }
                        },
                        error: function (result) {

                        },
                        complete: function () {
                        }
                    });
                }
                else {
                    $.toast({
                        heading: "[Error " + (result.statusCode / 10) + "x] Login failed",
                        text: result.message + (result.data != "" ? ("<br>" + JSON.stringify(result.data)) : ""),
                        icon: 'error',
                        showHideTransition: 'slide',
                        position: 'bottom-right',
                    });
                }
            },
            error: function (result) {
                console.log(result);
                $.toast({
                    heading: "Error",
                    text: JSON.stringify(result),
                    icon: 'error',
                    showHideTransition: 'slide',
                    position: 'bottom-right',
                });
            },
            complete: function () {
                $("#btn-submit-login").prop("disabled", false);
            }
        });
    });
    $("#btn-connect-submit").on("click", function () {
        $("#btn-connect-submit").prop("disabled", true);
        $.ajax({
            type: "POST",
            url: "/saveNASConfig/",
            data: {
                "Host": $("#form-connect-host").val(),
                "Sharename": $("#form-connect-sharename").val(),
                "Username": $("#form-connect-username").val(),
                "Password": $("#form-connect-password").val()
            },
            success: function (result) {
                if (result.statusCode == 200) {
                    
                    $.toast({
                        heading: "NAS connected",
                        text: "",
                        icon: 'success',
                        showHideTransition: 'slide',
                        position: 'bottom-right',
                        afterHidden: function () {
                            
                        }
                    });
                }
                else {
                    $.toast({
                        heading: "Error " + (result.statusCode / 10) + "x",
                        text: result.message + (result.data != "" ? ("<br>" + JSON.stringify(result.data)) : ""),
                        icon: 'error',
                        showHideTransition: 'slide',
                        position: 'bottom-right',
                    });
                }
            },
            error: function (result) {
                console.log(result);
                $.toast({
                    heading: "Error",
                    text: JSON.stringify(result),
                    icon: 'error',
                    showHideTransition: 'slide',
                    position: 'bottom-right',
                });
            },
            complete: function () {
                $("#btn-connect-submit").prop("disabled", false);
            }
        });
    });

});
myAudio.addEventListener("play", (event) => {
    myAudioState = 1;
        console.log("play handle");
});
myAudio.addEventListener("pause", (event) => {
    myAudioState = 0;
        console.log("pause handle");
});
myAudio.addEventListener("ended", (event) => {
    myAudioState = 0;
    console.log("stop handle");
});
$(document).on("click", ".btnPlay", function (element) {
    var ID = $(element.target).data("id");
    if (ID != myAudioFile) {
        myAudioFile = ID;
        myAudio.pause();
        myAudio = new Audio('/getCDR/' + myAudioFile);
        myAudio.addEventListener("canplaythrough", (event) => {
            myAudio.play();
            myAudioState = 1;
            $($(element.target).parents('tr').find('td.cell-durration')[0]).text(myAudio.duration+"s");
        });
    }
    else {
        if (myAudioState == 1) {
            myAudio.pause();
            myAudioState = 0;
        }
        else {
            myAudio.currentTime = 0;
            myAudio.play();
            myAudioState = 1;
        }
    }
});
function setInputFilter(textbox, inputFilter, errMsg) {
    ["input", "keydown", "keyup", "mousedown", "mouseup", "select", "contextmenu", "drop", "focusout"].forEach(function (event) {
        textbox.addEventListener(event, function (e) {
            if (inputFilter(this.value)) {
                // Accepted value
                if (["keydown", "mousedown", "focusout"].indexOf(e.type) >= 0) {
                    this.classList.remove("input-error");
                    this.setCustomValidity("");
                }
                this.oldValue = this.value;
                this.oldSelectionStart = this.selectionStart;
                this.oldSelectionEnd = this.selectionEnd;
            } else if (this.hasOwnProperty("oldValue")) {
                // Rejected value - restore the previous one
                this.classList.add("input-error");
                this.setCustomValidity(errMsg);
                this.reportValidity();
                this.value = this.oldValue;
                this.setSelectionRange(this.oldSelectionStart, this.oldSelectionEnd);
            } else {
                // Rejected value - nothing to restore
                this.value = "";
            }
        });
    });
}
$(function () {
    "use strict";

    const options = {
        containers: ["#swup", "#swupMenu"],
        animateHistoryBrowsing: true,
        linkSelector: 'a:not([data-no-swup])'
    };
    const swup = new Swup(options);

    // scrollbar
    Scrollbar.use(OverscrollPlugin);
    Scrollbar.init(document.querySelector('#scrollbar'), {
        damping: 0.05,
        renderByPixel: true,
        continuousScrolling: true,
    });
    Scrollbar.init(document.querySelector('#scrollbar2'), {
        damping: 0.05,
        renderByPixel: true,
        continuousScrolling: true,
    });

    // page loading
    $(document).ready(function () {
        anime({
            targets: '.art-preloader .art-preloader-content',
            opacity: [0, 1],
            delay: 200,
            duration: 600,
            easing: 'linear',
            complete: function (anim) {

            }
        });
        anime({
            targets: '.art-preloader',
            opacity: [1, 0],
            delay: 2200,
            duration: 400,
            easing: 'linear',
            complete: function (anim) {
                $('.art-preloader').css('display', 'none');
            }
        });
    });

    var bar = new ProgressBar.Line(preloader, {
        strokeWidth: 1.7,
        easing: 'easeInOut',
        duration: 1400,
        delay: 750,
        trailWidth: 1.7,
        svgStyle: {
            width: '100%',
            height: '100%'
        },
        step: (state, bar) => {
            bar.setText(Math.round(bar.value() * 100) + ' %');
        }
    });

    bar.animate(1);
    $('.art-input').keyup(function () {
        if ($(this).val()) {
            $(this).addClass('art-active');
        } else {
            $(this).removeClass('art-active');
        }
    });
    $('.current-menu-item a').clone().appendTo('.art-current-page');

    $('.art-map-overlay').on('click', function () {
        $(this).addClass('art-active');
    });
    $('.art-info-bar-btn').on('click', function () {
        $('.art-info-bar').toggleClass('art-active');
        $('.art-menu-bar-btn').toggleClass('art-disabled');
    });
    $('.art-menu-bar-btn').on('click', function () {
        $('.art-menu-bar-btn , .art-menu-bar').toggleClass("art-active");
        $('.art-info-bar-btn').toggleClass('art-disabled');
    });
    $('.art-info-bar-btn , .art-menu-bar-btn').on('click', function () {
        $('.art-content').toggleClass('art-active');
    });
    $('.art-curtain , .art-mobile-top-bar').on('click', function () {
        $('.art-menu-bar-btn , .art-menu-bar , .art-info-bar , .art-content , .art-menu-bar-btn , .art-info-bar-btn').removeClass('art-active , art-disabled');
    });
    $('.menu-item').on('click', function () {
        if ($(this).hasClass('menu-item-has-children')) {
            $(this).children('.sub-menu').toggleClass('art-active');
        } else {
            $('.art-menu-bar-btn , .art-menu-bar , .art-info-bar , .art-content , .art-menu-bar-btn , .art-info-bar-btn').removeClass('art-active , art-disabled');
        }
    });
    // reinit
    document.addEventListener("swup:contentReplaced", function () {
        Scrollbar.use(OverscrollPlugin);
        Scrollbar.init(document.querySelector('#scrollbar'), {
            damping: 0.05,
            renderByPixel: true,
            continuousScrolling: true,
        });
        Scrollbar.init(document.querySelector('#scrollbar2'), {
            damping: 0.05,
            renderByPixel: true,
            continuousScrolling: true,
        });
        $('.art-input').keyup(function () {
            if ($(this).val()) {
                $(this).addClass('art-active');
            } else {
                $(this).removeClass('art-active');
            }
        });
        $('.current-menu-item a').clone().prependTo('.art-current-page');
        $('.menu-item').on('click', function () {
            if ($(this).hasClass('menu-item-has-children')) {
                $(this).children('.sub-menu').toggleClass('art-active');
            } else {
                $('.art-menu-bar-btn , .art-menu-bar , .art-info-bar , .art-content , .art-menu-bar-btn , .art-info-bar-btn').removeClass('art-active , art-disabled');
            }
        });
    })
});

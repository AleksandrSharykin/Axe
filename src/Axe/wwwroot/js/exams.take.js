$(document).ready(function () {
    var examForm = document.getElementById("examForm");
    if (examForm) {
        // submitting intermediate values in background every 12 sec for monitoring
        setInterval(function () {
            var d = $('#examForm').serialize();

            $.ajax({
                url: ('/Exams/Monitor'),
                type: 'POST',
                dataType: 'json',
                data: d
            })

            //var xq = new XMLHttpRequest();
            //xq.open('post', '/Exams/Monitor', true);
            //xq.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
            //xq.send(d);

        }, 12000)
    }

    $('#qNext').click(function () {
        changeSelectedTab(1);
    });

    $('#qPrevious').click(function () {
        changeSelectedTab(-1);
    });

    function getTabAnchor(t) {
        return $(t).children('a').first().attr('href');
    }

    function changeSelectedTab(dir) {

        var tabControl = $("#questionsTab");
        var selectedTab = tabControl.children('li.active')[0];
        var tabs = tabControl.children('li')
        var idx = tabs.index(selectedTab);
        var count = tabs.length;

        var contentHref = getTabAnchor(selectedTab);
        console.log(contentHref);

        var nextTab;
        if (dir > 0 && idx < count - 1) {
            var nextTab = tabs.eq(idx + 1);
        } else if (dir < 0 && idx > 0) {
            nextTab = tabs.eq(idx - 1);
        }

        if (nextTab) {
            $(selectedTab).removeClass('active');
            $(contentHref).removeClass('in active');

            nextTab.addClass('active');

            var contentHref = getTabAnchor(nextTab);
            $(contentHref).addClass('in active');

        }
    }

    $('li[draggable=true]').each(function () {
        this.addEventListener("dragstart", drag);

        var me = this;
        $(this).children('span.glyphicon-flash').click(function () {
            $(this).hide();

            var answerGroup = $(this).parents('div.answersGroup');
            // return item to options list
            var options = answerGroup.find('ul')[0];
            options.appendChild(me);
            $(me).attr('draggable', 'true');
            $(me).find('input.answerNumber').val(null);

            // update numbers of selected options
            var selectedOptions = answerGroup.find('ol').first();
            var numbers = selectedOptions.find('input.answerNumber');
            for (var idx = 0; idx < numbers.length; idx++) {
                $(numbers[idx]).val(idx + 1);
            }
        });
    });

    //https://www.w3schools.com/html/html5_draganddrop.asp
    function drag(ev) {
        // when starting drag, remember #id of moving element
        ev.dataTransfer.setData("text", ev.target.id);
    }

    $('.dropArea').each(function () {
        this.addEventListener("dragover", allowDrop);
        this.addEventListener("drop", drop);
    });

    function allowDrop(ev) {
        ev.preventDefault();
    }

    function drop(ev) {
        ev.preventDefault();

        var target = ev.target;

        if (!$(target).hasClass('dropArea')) {
            var parents = $(target).parents('ol.dropArea');
            console.log(parents);
            target = $(target).parents('ol.dropArea')[0];
            console.log(target);
        }

        if (!target) return;

        var data = ev.dataTransfer.getData("text");
        var item = $('#' + data);
        item.removeAttr('draggable');
        item = item.get(0);

        if (target === ev.target) {
            target.appendChild(item);
        }
        else {
            var current = ev.target;
            if (current.tagName != item.tagName) {
                current = $(current).parents('li')[0];
            }
            target.insertBefore(item, current);
        }

        var fields = $(target).children('li').children('input.answerNumber');
        for (var x = 0; x < fields.length; x++) {
            $(fields[x]).val(x + 1);
        }

        $(item).children('span.glyphicon-flash').show();
    }
});

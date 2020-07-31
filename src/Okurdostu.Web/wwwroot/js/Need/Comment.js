function MainComment() {
    var _NeedId = $("#maincomment-div input[name='NeedId']").val();
    var _Comment = $("#maincomment-div textarea[id='Comment']").val();

    if (_NeedId.length <= 0) {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1\">Bazı şeylere ulaşamadık. Sayfayı yenileyin, tekrar deneyin</span>"
        });
    }
    else if (_Comment.length <= 0) {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1\">Tartışma başlatmak istiyorsanız bir içerik girmelisiniz</span>"
        });
    }
    else if (_Comment.length > 100) {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1\">En fazla 100 karakter</span>"
        });
    }
    else {
        $.post("/api/comments", { NeedId: _NeedId, Comment: _Comment }).done(function (result) {
            $("#maincomment-div textarea[name='Comment']").val('');
            GetComments(result.data)
        }).fail(function (result) {
            Toast.fire({
                icon: "error",
                html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
            });
        });
    }
}


function getReplyDialog(id) {
    $.get("/api/comments/" + id).done(function (result) {
        $("#replycomment-div input[name='RelatedCommentId']").val(id);
        $("#replying-comment").html(result.data.comment);
        $("#replying-user-fullname").html(result.data.fullname);
        $("#repyling-user-username").html("(\u0040" + result.data.username + ")");
        $('#reply-comment-modal').modal('show');
    });
};
function ReplyComment() {
    var _RelatedCommentId = $("#replycomment-div input[name='RelatedCommentId']").val();
    var _Comment = $("#replycomment-div textarea[name='ReplyComment']").val();

    if (_RelatedCommentId.length <= 0) {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1\">Bazı şeylere ulaşamadık. Sayfayı yenileyin, tekrar deneyin</span>"
        });
    }
    else if (_Comment.length <= 0) {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1\">Cevap yazmak istiyorsanız bir içerik girmelisiniz</span>"
        });
    }
    else if (_Comment.length > 100) {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1\">En fazla 100 karakter</span>"
        });
    }
    else {
        $.post("/api/comments", { RelatedCommentId: _RelatedCommentId, Comment: _Comment }).done(function (result) {
            $("#replycomment-div textarea[name='ReplyComment']").val('');
            $('#reply-comment-modal').modal('hide');
            GetComments(result.data);
        }).fail(function (result) {
            Toast.fire({
                icon: "error",
                html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
            });
        });
    }
}


var EditingCommentId;
var EditingComment;
function getEditDialog(id) {
    EditingCommentId = id;
    $("#edit-comment-input").val($("p[id='" + EditingCommentId + "']").text());
    EditingComment = $("p[id='" + EditingCommentId + "']").text();
    setTimeout(
        function () {
            $("#edit-comment-modal").modal('show');
        }, 100);
};
function EditComment() {
    var NewComment = $("#edit-comment-input").val();

    if (NewComment == EditingComment) {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1\">Aynı içeriği giriyorsunuz</span>"
        });
    }
    else if (NewComment.length <= 0) {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1\">Düzenlemek istiyorsanız bir içerik girmelisiniz</span>"
        });
    }
    else if (NewComment.length > 100) {
        Toast.fire({
            icon: "info",
            html: "<span class=\"font-weight-bold text-black-50 ml-1\">En fazla 100 karakter</span>"
        });
    }
    else {
        $.ajax({
            url: "/api/comments/" + EditingCommentId,
            data: { Comment: NewComment },
            type: 'Patch',
            success: function (result) {
                $("p[id='" + EditingCommentId + "']").text(NewComment);
                $('#edit-comment-modal').modal('hide');
                Toast.fire({
                    icon: "success",
                    html: "<span class=\"font-weight-bold text-black-50 ml-1\">" + result.message + "</span>"
                });
            },
            error: function () {
                Toast.fire({
                    icon: "error",
                    html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
                });
            }
        });
    }
}

var DeletingCommentId;
function getDeleteDialog(id) {
    DeletingCommentId = id;
    $('#delete-comment-modal').modal('show');
}

function DeleteComment() {
    $.ajax({
        url: "/api/comments/remove/" + DeletingCommentId,
        type: 'Patch',
        success: function (result) {
            GetComments();
            $('#delete-comment-modal').modal('hide');
            Toast.fire({
                icon: "success",
                html: "<span class=\"font-weight-bold text-black-50 ml-1\">" + result.message + "</span>"
            });
        },
        error: function () {
            Toast.fire({
                icon: "error",
                html: "<span class=\"font-weight-bold text-black-50 ml-1 \">" + result.responseJSON.message + "</span>"
            });
        }
    });
}
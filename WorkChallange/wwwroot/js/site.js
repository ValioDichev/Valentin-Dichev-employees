// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('#fileForm').submit(function (event) {
        event.preventDefault();

        var fileName = $('#fileInput').val().split('\\').pop();

        // Define forbidden characters
        var forbiddenCharacters = ['?', '%', '*', ':', '|', '/', '\\', '<', '>'];

        // Check if the file name contains any forbidden characters
        var containsForbiddenCharacters = forbiddenCharacters.some(function (char) {
            return fileName.includes(char);
        });

        if (containsForbiddenCharacters) {
            $('#errorText').text('File name cannot contain the following characters: ? % * : | / \\ < >');
            return;
        }

        $('#fileForm')[0].submit();
    });
});
$(document).ready(function () {
    $('#dueDate').datepicker({
        format: 'dd/mm/yyyy', 
        autoclose: true
    }).on('changeDate', function (e) {
        const selectedDate = e.format('dd/mm/yyyy');
        $('#selectedDate').text('Selected Date: ' + selectedDate);
    });
});
function openDatepicker() {
    console.log("Opening datepicker");
    $('#dueDate').datepicker('show');
}
$(document).ready(function () {

    $(document).on('click', '.btn-save', function () {
        var row = $(this).closest('tr');
        var itemId = row.data('item-id');
        var updatedData = {
            Id: itemId,
            EntryItem: row.find('[data-field="EntryItem"]').text().trim(),
            EntryDescription: row.find('[data-field="EntryDescription"]').text().trim(),
            DueDate: row.find('[data-field="DueDate"]').val() 
        };

        $.ajax({
            url: '/update-entry',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(updatedData),
            success: function (response) {
                if (response.success) {
                    $('#successMessage').text("Item updated successfully !").fadeIn().delay(2000).fadeOut();
                } else {
                    $('#errorMessage').text(response.message).fadeIn().delay(2000).fadeOut();
                }
            },
            error: function () {
                $('#errorMessage').text("An unexpected error occurred.").fadeIn().delay(2000).fadeOut();
            }
        });
    });
});
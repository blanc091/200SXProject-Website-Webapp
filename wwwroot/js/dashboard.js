// Ensure document is fully loaded
$(document).ready(function () {
    // Initialize the datepicker on the #dueDate input
    $('#dueDate').datepicker({
        format: 'dd/mm/yyyy',  // Set the desired format to dd/mm/yyyy
        autoclose: true
    }).on('changeDate', function (e) {
        // Update the label with the selected date in dd/mm/yyyy format
        const selectedDate = e.format('dd/mm/yyyy');
        $('#selectedDate').text('Selected Date: ' + selectedDate);
    });
});

// Function to open the datepicker when the button is clicked
function openDatepicker() {
    console.log("Opening datepicker");
    $('#dueDate').datepicker('show');
}

// Handle the Save button click
$(document).ready(function () {

    // Handle the Save button click
    $(document).on('click', '.btn-save', function () {
        var row = $(this).closest('tr');
        var itemId = row.data('item-id');

        // Prepare the data to send to the server
        var updatedData = {
            Id: itemId,
            EntryItem: row.find('[data-field="EntryItem"]').text().trim(),
            EntryDescription: row.find('[data-field="EntryDescription"]').text().trim(),
            DueDate: row.find('[data-field="DueDate"]').val()  // Date should be sent as string
        };

        // Send the AJAX request
        $.ajax({
            url: '/Dashboard/UpdateEntry',  // This needs to match the action in your controller
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(updatedData),
            success: function (response) {
                if (response.success) {
                    $('#successMessage').fadeIn().delay(2000).fadeOut();
                } else {
                    $('#errorMessage').fadeIn().delay(2000).fadeOut();
                }
            },
            error: function () {
                $('#errorMessage').fadeIn().delay(2000).fadeOut();
            }
        });
    });
});
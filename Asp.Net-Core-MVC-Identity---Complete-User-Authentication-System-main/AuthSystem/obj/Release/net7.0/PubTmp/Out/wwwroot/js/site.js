
//SEARABLE DROPDOWN



function AddItem(btn) {

    console.log("Adding item...");

    var table;
    table = document.getElementById('CodesTable');
    var rows = table.getElementsByTagName('tr');

    // CHECK QUANTITY >= 0 THEN
    var invalidQuantity = false;
    var quantityInputs = document.querySelectorAll('.quantity-input');
    quantityInputs.forEach(function (input) {
        var enteredQuantity = parseFloat(input.value);
        if (enteredQuantity <= 0 || isNaN(enteredQuantity)) {
            invalidQuantity = true;
            return;
        }
    });

    if (invalidQuantity) {
        Swal.fire({
            icon: 'error',
            title: 'Invalid Quantity',
            text: 'Quantity should be greater than 0 for all rows!',
        });
        return;
    }

   //FOR ADD NEW ROW
    var rowOuterHtml = rows[rows.length - 1].outerHTML;
    var lastrowIdx = rows.length - 2;
    var nextrowIdx = eval(lastrowIdx) + 1;

    rowOuterHtml = rowOuterHtml.replaceAll('_' + lastrowIdx + '_', '_' + nextrowIdx + '_');
    rowOuterHtml = rowOuterHtml.replaceAll('[' + lastrowIdx + ']', '[' + nextrowIdx + ']');
    rowOuterHtml = rowOuterHtml.replaceAll('-' + lastrowIdx, '-' + nextrowIdx);

    var newRow = table.insertRow();
    newRow.innerHTML = rowOuterHtml;

    //var x = document.getElementsByTagName("INPUT");

    //for (var cnt = 0; cnt < x.length; cnt++) {
    //    if (x[cnt].type == "text" && x[cnt].id.indexOf('_' + nextrowIdx + '_') > 0) {
    //        x[cnt].value = '';

    //    }
    //    else if (x[cnt].type == "number" && x[cnt].id.indexOf('_' + nextrowIdx + '_') > 0)
    //        x[cnt].value = 0;
    //}

    var x = document.querySelectorAll("#CodesTable input, #CodesTable select");
    for (var cnt = 0; cnt < x.length; cnt++) {
        var element = x[cnt];
        var id = element.id;
        if (id.indexOf('_' + nextrowIdx + '_') > 0) {
            if (element.tagName.toLowerCase() === "input") {
                if (element.type === "text") {
                    element.value = '';
                } else if (element.type === "number") {
                    element.value = 0;
                }
            } else if (element.tagName.toLowerCase() === "select") {
                element.selectedIndex = 0; // Reset select to first option
            }
        }
    }

    //  CHECK QUANTITY <= 0 THEN
    var newQuantityInputs = newRow.querySelectorAll('.quantity-input');
    newQuantityInputs.forEach(function (newInput) {
        newInput.addEventListener('change', function () {
            var enteredQuantity = parseFloat(this.value);
            if (enteredQuantity <= 0) {
                Swal.fire({
                    icon: 'error',
                    title: 'Invalid Quantity',
                    text: 'Quantity cannot be less than 0 or Equal to 0 !',
                });
                this.value = '0';
            }
        });
    });
    // END

    rebindvalidators();
}
function AddItem1(btn) {
    var table = document.getElementById('CodesTable');
    var rows = table.getElementsByTagName('tr');
    var rowOuterHtml = rows[rows.length - 1].outerHTML;

    // Create a temporary container to parse the HTML
    var container = document.createElement('div');
    container.innerHTML = rowOuterHtml;

    // Find all input elements within the container
    var inputElements = container.querySelectorAll('input');

    // Iterate through input elements and get their values
    inputElements.forEach(function (inputElement) {
        var id = inputElement.id;
        var value = inputElement.value;
        console.log('Input ID:', id);
        console.log('Input Value:', value);
    });

    var lastrowIdx = rows.length - 2;
    var nextrowIdx = eval(lastrowIdx) + 1;
    console.log("lastrowIdx :" + lastrowIdx);
    console.log("nextrowIdx :" + nextrowIdx);
    // Replace placeholders in the HTML for the new row
    rowOuterHtml = rowOuterHtml.replaceAll('_' + lastrowIdx + '_', '_' + nextrowIdx + '_');
    rowOuterHtml = rowOuterHtml.replaceAll('[' + lastrowIdx + ']', '[' + nextrowIdx + ']');
    rowOuterHtml = rowOuterHtml.replaceAll('-' + lastrowIdx, '-' + nextrowIdx);

    // Append the updated row to the table
    var newRow = table.insertRow();
    newRow.innerHTML = rowOuterHtml;
    console.log(rowOuterHtml);
    // Reset the new row's input values
    for (var cnt = 0; cnt < x.length; cnt++) {
        console.log("new :" + x[cnt].id);
        if (x[cnt].type == "text" && x[cnt].id.indexOf('_' + nextrowIdx + '_') > 0) {
            x[cnt].value = '';

        }
        else if (x[cnt].type == "number" && x[cnt].id.indexOf('_' + nextrowIdx + '_') > 0)
            x[cnt].value = 0;
    }
    rebindvalidators();
}

//function AddItem1(btn) {
//    var table = document.getElementById('CodesTable');
//    var rows = table.getElementsByTagName('tr');
//    var rowOuterHtml = rows[rows.length - 1].outerHTML;

//    // Create a temporary container to parse the HTML
//    var container = document.createElement('div');
//    container.innerHTML = rowOuterHtml;

//    // Find all input elements within the container
//    var inputElements = container.querySelectorAll('input');

//    // Iterate through input elements and get their values
//    inputElements.forEach(function (inputElement) {
//        var id = inputElement.id;
//        var value = inputElement.value;
//        console.log('Input ID:', id);
//        console.log('Input Value:', value);
//    });

//    var lastrowIdx = rows.length - 2;
//    var nextrowIdx = eval(lastrowIdx) + 1;
//    console.log("lastrowIdx :" + lastrowIdx);
//    console.log("nextrowIdx :" + nextrowIdx);
//    // Replace placeholders in the HTML for the new row
//    rowOuterHtml = rowOuterHtml.replaceAll('_' + lastrowIdx + '_', '_' + nextrowIdx + '_');
//    rowOuterHtml = rowOuterHtml.replaceAll('[' + lastrowIdx + ']', '[' + nextrowIdx + ']');
//    rowOuterHtml = rowOuterHtml.replaceAll('-' + lastrowIdx, '-' + nextrowIdx);

//    // Append the updated row to the table
//    var newRow = table.insertRow();
//    newRow.innerHTML = rowOuterHtml;
//    console.log(rowOuterHtml);
//    // Reset the new row's input values
//       for (var cnt = 0; cnt < x.length; cnt++) {
//        console.log("new :" + x[cnt].id);
//            if (x[cnt].type == "text" && x[cnt].id.indexOf('_' + nextrowIdx + '_') > 0) {
//                x[cnt].value = '';

//            }
//        else if (x[cnt].type == "number" && x[cnt].id.indexOf('_' + nextrowIdx + '_') > 0)
//            x[cnt].value = 0;
//           }
//    rebindvalidators();
//}
function rebindvalidators() {

    var $form = $("#CodeSbyAnizForm");
    $form.unbind();
    $form.data("validator", null);
    $.validator.unobtrusive.parse($form);
    $form.validate($form.data("unobtrusiveValidation").options);

    var $form1 = $("#form1");
    $form1.unbind();
    $form1.data("validator", null);
    $.validator.unobtrusive.parse($form1);
    $form1.validate($form.data("unobtrusiveValidation").options);
}
function DeleteItem(btn) {

    var table = document.getElementById('CodesTable');
    var rows = table.getElementsByTagName('tr');
    console.log("rows : " + rows);
    var btnIdx = btn.id.replaceAll('btnremove-', '');
    // var idOfQuantity = btnIdx + "__qty";
    // var txtQuantity = document.querySelector("[id$='" + idOfQuantity + "']");

    // txtQuantity.value = 0;


    var idOfIsDeleted = btnIdx + "__IsDeleted";
    var txtIsDeleted = document.querySelector("[id$='" + idOfIsDeleted + "']");

    txtIsDeleted.value = "true";


    if (btnIdx > 0) {
        $(btn).closest('tr').remove();
        console.log("btn ondex : " + btnIdx);
    }
    //CalcTotals();

}

//CREATE SUCCESSFULLY 
function ConfirmCreate(ev) {
    if (object.status) {
        return true;
    }
    swal({
        title: 'Record Created Successfully',
        type: 'success',
        showCancelButton: false,
        confirmButtonColor: '#3085d6',
        confirmButtonText: 'OK',
        confirmButtonClass: 'btn btn-success',
    },
        function (isConfirmed) {
            if (isConfirmed) {
                object.status = true;
                object.ele = ev;
                object.ele.click();
                swal("Record Created Successfully");
            }
        });
    return false;
}

// UPDATE SUCCESSFULLY
function ConfirmEdit(ev) {
    if (object.status) {
        return true;
    }
    swal({
        title: 'Record Updated Successfully',
        type: 'success',
        showCancelButton: false,
        confirmButtonColor: '#3085d6',
        confirmButtonText: 'OK',
        confirmButtonClass: 'btn btn-success',
    },
        function (isConfirmed) {
            if (isConfirmed) {
                object.status = true;
                object.ele = ev;
                object.ele.click();
                swal("Record Updated Successfully");
            }
        });
    return false;
}

//STATUS CHANGED SUCCESSFULLY 
function ConfirmStatus(ev) {
    if (object.status) {
        return true;
    }
    swal({
        title: 'Change Status',
        text: 'Do you want to change the status ?',
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes',
        cancelButtonText: 'No',
        confirmButtonClass: 'btn btn-success',
        cancelButtonClass: 'btn btn-danger',
    },
        function (isConfirmed) {
            if (isConfirmed) {
                object.status = true;
                object.ele = ev;
                object.ele.click();
                // Display the status change message
                swal("Status Change updated ! ");
            }
        });
    return false;
}

//DELETED SUCCESSFULLY 
function ConfirmDelete(ev) {
    if (object.status) {
        return true;
    }
    swal({
        title: ' Delete Record',
        text: 'Do you want to delete the record ?',
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes',
        cancelButtonText: 'No',
        confirmButtonClass: 'btn btn-success',
        cancelButtonClass: 'btn btn-danger',
    },
        function (isConfirmed) {
            if (isConfirmed) {
                object.status = true;
                object.ele = ev;
                object.ele.click();
                // Display the status change message
                swal("Record Deleted Successfully ! ");
            } else {
                object.status = false;
                object.ele = ev;
                object.ele.click();
                // Display the status change message
                swal("Deletion Cancel ! ");
            }
        });
    return false;
}

//$(function () {
//    $(".selectdrop").select2();  
//    $(".replacement").select2();  
//    $("#sonoId").select2();
//    $("#suppliernameId").select2();
//    $("#customernameId").select2();
//    $(".select2").select2();
//    $(".descriptionn").select2();
//    $('#dataTable').DataTable();
//    $("#loaderbody").addClass('hide');
//});

showInPopup = (url, title) => {
    // Clear modal content when it's hidden
    $('#form-modal .modal-body').html('');
    $('#form-modal .modal-title').html('');
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            console.log(res);
            if (res.message === "Sweet") {
                console.log(res);
                showAlert(res);
            } else if (res === "AlreadyAdded") {
                console.log(res);
                AlreadyAdded();
            } else if (res === "succesfullysradded") {
                console.log(res);
                succesfullysradded();
            } else {
                console.log(res);
                $('#form-modal .modal-body').html(res);
                $('#form-modal .modal-title').html(title);
                $('#form-modal').modal('show');
                fetchAndDisplayListData();
                //fetchAndDisplayListData1();
                //fetchAndDisplayListDataso();
            }
           
        }
    })
}

showInPopup1 = (url, title) => {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            console.log(res);
            if (res.message === "Sweet") {
                console.log(res);
                showAlert(res);
            } else if (res === "AlreadyAdded") {
                console.log(res);
                AlreadyAdded();
            } else if (res === "succesfullysradded") {
                console.log(res);
                succesfullysradded();
            } else {
                console.log(res);
                $('#form-modal .modal-body').html(res);
                $('#form-modal .modal-title').html(title);
                $('#form-modal').modal('show');
                //fetchAndDisplayListData();
                fetchAndDisplayListData1();
                //fetchAndDisplayListDataso();
            }

        }
    })
}

showInPopupso = (url, title) => {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            console.log(res);
            if (res.message === "Sweet") {
                console.log(res);
                showAlert(res);
            } else if (res === "AlreadyAdded") {
                console.log(res);
                AlreadyAdded();
            } else if (res === "succesfullysradded") {
                console.log(res);
                succesfullysradded();
            } else {
                console.log(res);
                $('#form-modal .modal-body').html(res);
                $('#form-modal .modal-title').html(title);
                $('#form-modal').modal('show');
                //fetchAndDisplayListData();
                //fetchAndDisplayListData1();
                fetchAndDisplayListDataso();
            }

        }
    })
}
function fetchAndDisplayListData() {
    var pono = $('#pono').val();
    var productCode = $('#productcode').val();
    var quantity = $('#quantity').val();
    var warranty = $('#warranty').val();

    $.ajax({
        type: 'GET',
        url: '/inwards/ShowItem?pono=' + pono + '&productCode=' + productCode + '&quantity=' + quantity + '&warranty=' + warranty,
        success: function (response) {
            displayTable(response.data);
        },
        error: function (error) {
            console.error('Error fetching data:', error);
        }
    });
}
function fetchAndDisplayListData1() {
    var pono = $('#dcno').val();
    var pono = $('#invoiceno').val();
    var productCode = $('#productcode').val();
    var quantity = $('#quantity').val();
    var warranty = $('#warranty').val();

    $.ajax({
        type: 'GET',
        url: '/inwards/ShowItem1?dcno=' + dcno + '&invoiceno=' + invoiceno + '&productCode=' + productCode + '&quantity=' + quantity + '&warranty=' + warranty,
        success: function (response) {
            displayTable1(response.data);
        },
        error: function (error) {
            console.error('Error fetching data:', error);
        }
    });
}
function fetchAndDisplayListDataso() {
    var sono = $('#sono').val();
    var productCode = $('#productcode').val();
    var quantity = $('#quantity').val();
    var warranty = $('#warranty').val();

    $.ajax({
        type: 'GET',
        url: '/inwards/ShowItemso?sono=' + sono + '&productCode=' + productCode + '&quantity=' + quantity + '&warranty=' + warranty,
        success: function (response) {
            displayTableso(response.data);
        },
        error: function (error) {
            console.error('Error fetching data:', error);
        }
    });
}

showInPopupO = (url, title) => {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            console.log(res);
            if (res.message === "Sweet") {
                console.log(res);
                showAlert(res);
            } else if (res === "AlreadyAdded") {
                console.log(res);
                AlreadyAdded();
            } else if (res === "succesfullysradded") {
                console.log(res);
                succesfullysradded();
            } else {
                console.log(res);
                $('#form-modal .modal-body').html(res);
                $('#form-modal .modal-title').html(title);
                $('#form-modal').modal('show');
                fetchAndDisplayListDataO();
                //fetchAndDisplayListData1();
                //fetchAndDisplayListDataso();
            }

        }
    })
}

showInPopup1O = (url, title) => {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            console.log(res);
            if (res.message === "Sweet") {
                console.log(res);
                showAlert(res);
            } else if (res === "AlreadyAdded") {
                console.log(res);
                AlreadyAdded();
            } else if (res === "succesfullysradded") {
                console.log(res);
                succesfullysradded();
            } else {
                console.log(res);
                $('#form-modal .modal-body').html(res);
                $('#form-modal .modal-title').html(title);
                $('#form-modal').modal('show');
                //fetchAndDisplayListData();
                fetchAndDisplayListData1O();
                //fetchAndDisplayListDataso();
            }

        }
    })
}

showInPopupsoO = (url, title) => {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            console.log(res);
            if (res.message === "Sweet") {
                console.log(res);
                showAlert(res);
            } else if (res === "AlreadyAdded") {
                console.log(res);
                AlreadyAdded();
            } else if (res === "succesfullysradded") {
                console.log(res);
                succesfullysradded();
            } else {
                console.log(res);
                $('#form-modal .modal-body').html(res);
                $('#form-modal .modal-title').html(title);
                $('#form-modal').modal('show');
                //fetchAndDisplayListData();
                //fetchAndDisplayListData1();
                fetchAndDisplayListDatasoO();
            }

        }
    })
}
function fetchAndDisplayListDataO() {
    var pono = $('#pono').val();
    var productCode = $('#productcode').val();
    var quantity = $('#quantity').val();
    var warranty = $('#warranty').val();

    $.ajax({
        type: 'GET',
        url: '/Outwards/ShowItem?pono=' + pono + '&productCode=' + productCode + '&quantity=' + quantity + '&warranty=' + warranty,
        success: function (response) {
            displayTableO(response.data);
        },
        error: function (error) {
            console.error('Error fetching data:', error);
        }
    });
}
function fetchAndDisplayListData1O() {
    var pono = $('#dcno').val();
    var pono = $('#invoiceno').val();
    var productCode = $('#productcode').val();
    var quantity = $('#quantity').val();
    var warranty = $('#warranty').val();

    $.ajax({
        type: 'GET',
        url: '/Outwards/ShowItem1?dcno=' + dcno + '&invoiceno=' + invoiceno + '&productCode=' + productCode + '&quantity=' + quantity + '&warranty=' + warranty,
        success: function (response) {
            displayTable1O(response.data);
        },
        error: function (error) {
            console.error('Error fetching data:', error);
        }
    });
}
function fetchAndDisplayListDatasoO() {
    var sono = $('#sono').val();
    var productCode = $('#productcode').val();
    var quantity = $('#quantity').val();
    var warranty = $('#warranty').val();

    $.ajax({
        type: 'GET',
        url: '/Outwards/ShowItemso?sono=' + sono + '&productCode=' + productCode + '&quantity=' + quantity + '&warranty=' + warranty,
        success: function (response) {
            displayTablesoO(response.data);
        },
        error: function (error) {
            console.error('Error fetching data:', error);
        }
    });
}

// Function to handle "AlreadyAdded" scenario
function AlreadyAdded() {
    Swal.fire({
        title: 'Already Entered',
        text: 'You have already entered data based on the quantity.',
        icon: 'warning',
        confirmButtonText: 'OK', cancelButtonText: 'Edit'
    }).then((result) => {
        if (result.isConfirmed) {
            // Handle any additional actions if needed
        } else if (result.dismiss === Swal.DismissReason.cancel) {
            //return back to this action and show that 
        }
    });
}
function succesfullysradded() {
    console.log("hi");
    Swal.fire({
        title: "Success",
        text: "Serial Numbers Added successfully !",
        icon: "success",
        confirmButtonText: 'OK'
    });
}

//function showAlert(title, text, icon) {
//    Swal.fire({
//        title: "Warning",
//        text: "Already Exist ! Do You want to clear the list data ?",
//        icon: "Warning",
//        showCancelButton: true,
//        confirmButtonText: 'OK',
//        customClass: {
//            confirmButton: 'btn btn-success',
//        }
//    }).then((result) => {
//        if (result.isConfirmed) {
//            console.log('OK clicked');
//        }
//    });
//}
function showAlert(res) {
    Swal.fire({
        title: "Warning",
        text: "Already Exist ! Do You want to view the list data ?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: 'No',
        cancelButtonText: 'View List', // Changed the Cancel button text to 'View List'
        customClass: {
            confirmButton: 'btn btn-success',
            cancelButton: 'btn btn-warning' // Adding a CSS class to style the Cancel/View List button
        }
            //buttonsStyling: false // Disable the default button styling to apply custom classes properly
    }).then((result) => {
        if (result.isConfirmed) {
            console.log('OK clicked, clearing list data...');
        } else if (result.dismiss === Swal.DismissReason.cancel) {
           console.log('View List clicked');
            viewListData(res);
        }
    });
}

//function viewListData() {
//    console.log('View List Data clicked');
//    console.log(serialno);
//    // alert(JSON.stringify(serialno));
//}

jQueryAjaxPost = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-all').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxDelete = form => {
    if (confirm('Are you sure to delete this record ?')) {
        try {
            $.ajax({
                type: 'POST',
                url: form.action,
                data: new FormData(form),
                contentType: false,
                processData: false,
                success: function (res) {
                    $('#view-all').html(res.html);
                },
                error: function (err) {
                    console.log(err)
                }
            })
        } catch (ex) {
            console.log(ex)
        }
    }

    //prevent default form submit event
    return false;
}
function updateValues(row) {
    var quantity = parseFloat(row.find('.quantity-input').val());
    var tqtySum = 0;

    // Check if the quantity has actually changed
    row.find('#CsDiv tr').each(function ()
    {
            var sqty = parseFloat($(this).find('input[name="Model.inwardPacket[i].sqty"]').val());
            var tqty = quantity * sqty;
            $(this).find('input[name="Model.inwardPacket[i].tqty"]').val(tqty.toFixed(2));
            tqtySum += tqty;
        });
        // Update totalsubassembly in the main table and multiply by the sum of all sqty
        row.find('.totalsubassembly').val(tqtySum.toFixed(2));
    }


$(function() {
    console.log("loading Function call");
    
});




var dataTable;


$(function () { //using jquery
    var url = window.location.search;//getting the complete url 
    if (url.includes("inprocess")) {
        loadDataTable("inprocess")
    }
    else if (url.includes("pending")) {
        loadDataTable("pending")
    }
    if (url.includes("approved")) {
        loadDataTable("approved")
    }
    if (url.includes("completed")) {
        loadDataTable("completed")
    }
    if (url.includes("all")) {
        loadDataTable("all")
    }
    
});


function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/order/getall?status='+status }, // We need to pass the status here 
        "columns": [
            { data: 'id', "width" :"5%"},
            { data: 'name', "width": "25%" },
            { data: 'phoneNumber', "width": "20%" },
            { data: 'applicationUser.email', "width": "25%" },
            { data: 'orderStatus', "width": "10%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'id',
                "render": function (data) { //You can specify a custom function
                    return `<div class="w-75 btn-group" role="group">
                            <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i></a>
                            
                    </div>`
                },
                "width": "10%"
            }
            
        ]
    });
}





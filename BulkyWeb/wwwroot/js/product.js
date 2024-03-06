

var dataTable;


$(function () { //using jquery
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/product/getall' },
        "columns": [
            { data: 'title', "width" :"25%"},
            { data: 'isbn', "width": "15%" },
            { data: 'listPrice', "width": "10%" },
            { data: 'author', "width": "15%" },
            { data: 'category.name', "width": "10%" },
            {
                data: 'id',
                "render": function (data) { //You can specify a custom function
                    return `<div class="w-75 btn-group" role="group">
                            <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i>Edit</a>
                            <a onClick=Delete('/admin/product/delete/${data}') class="btn btn-danger mx-2" ><i class="bi bi-trash-fill"></i>Delete</a>
                    </div>`
                },
                "width": "25%"
            }
            
        ]
    });
}


function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => { //We want to make an ajax request to our controller to delete the product
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) { // We are returing success and a message and that will be available in data 
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    });
}


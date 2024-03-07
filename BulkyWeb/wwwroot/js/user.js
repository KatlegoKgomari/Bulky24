


var dataTable;


$(function () { //using jquery
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/user/getall' },
        "columns": [
            { data: 'name', "width": "25%" },
            { data: 'email', "width": "15%" },
            { data: 'phoneNumber', "width": "10%" },
            { data: 'company.name', "width": "15%" },
            { data: 'role', "width": "10%" },
            {
                data: {id: 'id', lockoutEnd:'lockoutEnd'},
                "render": function (data) { //You can specify a custom function
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();
                    console.log(today)
                    console.log(lockout)
                    if (today>lockout) {
                        return `<div class="text-center">
                            <a onclick=LockUnlock('${data.id}') class="btn btn-success text-white" style="cursor:pointer; "> <i class="bi bi-unlock-fill"></i>UnLock</a>
                            <a href="/admin/user/RoleManagement?userId=${data.id}" class="btn btn-danger text-white" ><i class="bi bi-pencil-square" style="cursor:pointer; "></i>Permission</a>
                    </div>`}
                    else {
                        return `<div class="text-center">
                            <a onclick=LockUnlock('${data.id}') class="btn btn-danger text-white" style="cursor:pointer;"> <i class="bi bi-lock-fill"></i>Lock</a>
                            <a href="/admin/user/RoleManagement?userId=${data.id}" class="btn btn-danger text-white"style="cursor:pointer; " ><i class="bi bi-pencil-square"></i>Permission</a>
                    </div>`

                    }
                },
                "width": "25%"
            }

        ]
    });
}


function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
        }
    });
}



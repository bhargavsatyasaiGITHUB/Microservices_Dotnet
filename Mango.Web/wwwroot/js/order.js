﻿var dataTable;

$(document).ready(function () {
    loadDataTable();
})
function loadDataTable() {
    dataTable = $('#tblData').dataTable({
        "ajax": { url: "/order/getall" },
        "columns": [
            { data: 'orderHeaderId', "width": "5%"},
            { data: 'email', "width": "25%" },
            { data: 'name', "width": "25%" },
            { data: 'phone', "width": "10%" },
            { data: 'status', "width": "10%" },
            { data: 'ordertotal', "width": "10%" },
            {
                data: 'orderHeaderId',
                "render": function (data) {
                   return '<div class="w-75" btn-group role="group">
                    <a href="/order/orderDetail?orderId=${data}" class="btn btn-primary mx-2">i class="bi bi-pencil-sqaure></i></a>
                    </div>'
                },
                "width":"10%"
            }
        ]
    })
}
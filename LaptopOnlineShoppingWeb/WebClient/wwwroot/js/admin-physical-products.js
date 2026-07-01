let apiBase = document.body.getAttribute('data-api-base') || '';
let token = document.body.getAttribute('data-token') || '';
let sModal;

document.addEventListener("DOMContentLoaded", () => {
    sModal = new bootstrap.Modal(document.getElementById('statusModal'));
    loadBranches();
    loadPhysicalProducts();

    document.getElementById('filter_search').addEventListener('keypress', function(e) {
        if (e.key === 'Enter') loadPhysicalProducts();
    });
    
    document.getElementById('filter_status').addEventListener('change', loadPhysicalProducts);
    document.getElementById('filter_branch').addEventListener('change', loadPhysicalProducts);
});

const getHeaders = () => ({
    'Content-Type': 'application/json',
    'Authorization': 'Bearer ' + token
});

function loadBranches() {
    fetch(`${apiBase}/api/Branches`, {
        headers: getHeaders()
    })
        .then(res => res.json())
        .then(data => {
            const items = data.value || data;
            const select = document.getElementById('filter_branch');
            items.forEach(b => {
                const bId = b.branchId || b.BranchId;
                const bName = b.branchName || b.BranchName;
                select.innerHTML += `<option value="${bId}">${bName}</option>`;
            });
        })
        .catch(console.error);
}

function resetFilters() {
    document.getElementById('filter_search').value = '';
    document.getElementById('filter_status').value = '';
    document.getElementById('filter_branch').value = '';
    loadPhysicalProducts();
}

function getODataQuery() {
    let filters = [];
    const search = document.getElementById('filter_search').value.trim();
    const status = document.getElementById('filter_status').value;
    const branchId = document.getElementById('filter_branch').value;

    if (search) {
        filters.push(`contains(tolower(SerialNumber), tolower('${search}'))`);
    }
    if (status) {
        filters.push(`Status eq '${status}'`);
    }
    if (branchId) {
        filters.push(`BranchId eq ${branchId}`);
    }

    const filterQuery = filters.length > 0 ? `$filter=${filters.join(' and ')}&` : '';
    return `?${filterQuery}$expand=Variant($expand=Product),Branch,Order&$orderby=PhysicalId desc&$top=100`;
}

function loadPhysicalProducts() {
    const tbody = document.getElementById('physicalProductList');
    tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Đang tải dữ liệu...</td></tr>';

    fetch(`${apiBase}/api/PhysicalProducts${getODataQuery()}`, {
        headers: getHeaders()
    })
    .then(res => res.json())
    .then(data => {
        const items = data.value || data;
        tbody.innerHTML = '';
        if (items.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Không tìm thấy Seri nào phù hợp.</td></tr>';
            return;
        }

        items.forEach(p => {
            let statusBadge = '';
            switch(p.Status) {
                case 'InStock': statusBadge = '<span class="badge bg-success">Trong kho</span>'; break;
                case 'Sold': statusBadge = '<span class="badge bg-secondary">Đã bán</span>'; break;
                case 'Defective': statusBadge = '<span class="badge bg-danger">Lỗi/Hỏng</span>'; break;
                case 'Lost': statusBadge = '<span class="badge bg-dark">Thất lạc</span>'; break;
                default: statusBadge = `<span class="badge bg-info">${p.Status}</span>`; break;
            }

            const productName = p.Variant && p.Variant.Product ? p.Variant.Product.ProductName : 'N/A';
            const variantConfig = p.Variant ? `[CPU: ${p.Variant.CPU}, RAM: ${p.Variant.RAM}, SSD: ${p.Variant.SSD}]` : '';
            const branchName = p.Branch ? p.Branch.BranchName : 'N/A';
            const orderHtml = p.OrderId ? `<a href="/BackOffice/Orders/Details?id=${p.OrderId}">#${p.OrderId}</a>` : '<span class="text-muted">-</span>';

            tbody.innerHTML += `
                <tr>
                    <td class="fw-bold text-primary">${p.SerialNumber}</td>
                    <td>
                        <div class="fw-bold">${productName}</div>
                        <div class="small text-muted">${variantConfig}</div>
                    </td>
                    <td>${branchName}</td>
                    <td>${orderHtml}</td>
                    <td>${statusBadge}</td>
                    <td class="text-end">
                        <button class="btn btn-sm btn-outline-warning" onclick="openStatusModal(${p.PhysicalId}, '${p.SerialNumber}', '${p.Status}')">
                            <i class="bi bi-pencil-square"></i> Đổi trạng thái
                        </button>
                    </td>
                </tr>
            `;
        });
    })
    .catch(err => {
        console.error(err);
        tbody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Lỗi tải dữ liệu.</td></tr>';
    });
}

function openStatusModal(id, serial, currentStatus) {
    document.getElementById('modal_physicalId').value = id;
    document.getElementById('modal_serialNumber').textContent = serial;
    document.getElementById('modal_newStatus').value = currentStatus;
    sModal.show();
}

function saveStatus() {
    const id = document.getElementById('modal_physicalId').value;
    const newStatus = document.getElementById('modal_newStatus').value;

    fetch(`${apiBase}/api/PhysicalProducts/${id}/status`, {
        method: 'PUT',
        headers: getHeaders(),
        body: JSON.stringify(newStatus)
    })
    .then(async res => {
        if(res.ok) {
            Swal.fire('Thành công', 'Đã cập nhật trạng thái seri.', 'success');
            sModal.hide();
            loadPhysicalProducts();
        } else {
            const err = await res.json();
            Swal.fire('Lỗi', err.message || 'Không thể cập nhật trạng thái.', 'error');
        }
    })
    .catch(err => {
        console.error(err);
        Swal.fire('Lỗi', 'Đã có lỗi xảy ra khi kết nối server.', 'error');
    });
}

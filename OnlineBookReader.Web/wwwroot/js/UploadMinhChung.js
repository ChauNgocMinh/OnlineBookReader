document.getElementById("eventSelect").addEventListener("change", function () {
    var DotDanhGiaId = this.value;
    var MinhChungContainer = document.getElementById("MinhChungContainer");
    MinhChungContainer.innerHTML = "";
    if (!DotDanhGiaId) return;

    fetch(`/MinhChung/GetMinhChungsByDotDanhGia?DotDanhGiaId=${DotDanhGiaId}`)
        .then(response => response.json())
        .then(data => {
            if (!data || data.minhChungs.length === 0) {
                MinhChungContainer.innerHTML = "<p>Không có minh chứng nào cho đợt đánh giá này.</p>";
                return;
            }
            let moTaHtml = data.moTa ? data.moTa : "<p>Không có mô tả.</p>";
            MinhChungContainer.innerHTML = `
                <h5 >Tiêu chuẩn</h5>
                <div class="border p-3 mb-3 bg-light" style="overflow-y: auto; resize: vertical;">
                    ${moTaHtml}
                </div>
                <div id="minhChungTableContainer"></div> 
            `;


            fetch(`/MinhChung/GetUploadedFilesByDotDanhGia?DotDanhGiaId=${DotDanhGiaId}`)
                .then(response => response.json())
                .then(uploadedFiles => {
                    let tableHTML = `
                        <h5>Tự đánh giá</h5>
                        <table class="table table-bordered">
                            <thead>
                                <tr>
                                    <th>STT</th>
                                    <th>Nội dung đánh giá</th>
                                    <th>Kê khai minh chứng (nếu có)</th>
                                    <th>Đánh giá</th>
                                    <th>Thao tác</th>
                                </tr>
                            </thead>
                            <tbody>
                    `;

                    data.minhChungs.forEach((MinhChung, index) => {
                        let uploadedFile = uploadedFiles.find(f => f.minhChungId === MinhChung.id);
                        let fileCell = uploadedFile
                            ? `<a id="file-link-${MinhChung.id}" href="${uploadedFile.filePath}" target="_blank">
                                    <i class="fas fa-file-alt"></i> Xem file
                               </a>`
                            : `<input type="file" class="form-control file-input" data-minhchung-id="${MinhChung.id}">`;

                        let danhGiaValue = uploadedFile ? uploadedFile.danhGia : null;
                        let radioButtons = `
                           <div class="d-flex justify-content-center gap-3 align-items-center">
                                <label class="form-check-label">
                                    <input type="radio" name="danhGia-${MinhChung.id}" value="0" class="form-check-input" 
                                        ${danhGiaValue == 0 ? "checked" : ""}> Kém
                                </label>
                                <label class="form-check-label">
                                    <input type="radio" name="danhGia-${MinhChung.id}" value="1" class="form-check-input" 
                                        ${danhGiaValue == 1 ? "checked" : ""}> Trung bình 
                                </label>
                                <label class="form-check-label">
                                    <input type="radio" name="danhGia-${MinhChung.id}" value="2" class="form-check-input" 
                                        ${danhGiaValue == 2 ? "checked" : ""}> Tốt 
                                </label>
                                <label class="form-check-label">
                                    <input type="radio" name="danhGia-${MinhChung.id}" value="3" class="form-check-input" 
                                        ${danhGiaValue == 3 ? "checked" : ""}> Xuất sắc 
                                </label>
                            </div>
                        `;

                        let buttonAction = uploadedFile
                            ? `<button class="btn btn-danger delete-btn" data-file-id="${uploadedFile.id}" data-minhchung-id="${MinhChung.id}">🗑 Xóa</button>`
                            : `<button class="btn btn-success upload-btn" data-minhchung-id="${MinhChung.id}">📤 Tải lên</button>`;

                        tableHTML += `
                            <tr>
                                <td>${index + 1}</td>
                                <td>${MinhChung.ten}: ${MinhChung.moTa}</td>
                                <td>${fileCell}</td>
                                <td>${radioButtons}</td>
                                <td>${buttonAction}</td>
                            </tr>
                        `;
                    });

                    tableHTML += `</tbody></table>`;
                    document.getElementById("minhChungTableContainer").innerHTML = tableHTML;

                    document.querySelectorAll(".upload-btn").forEach(button => {
                        button.addEventListener("click", function (event) {
                            event.preventDefault();
                            var DotDanhGiaId = document.getElementById("eventSelect").value;
                            var MinhChungId = this.getAttribute("data-minhchung-id");
                            var fileInput = document.querySelector(`.file-input[data-minhchung-id="${MinhChungId}"]`);
                            var file = fileInput && fileInput.files ? fileInput.files[0] : null;
                            var danhGiaInput = document.querySelector(`input[name="danhGia-${MinhChungId}"]:checked`);
                            var danhGia = danhGiaInput ? danhGiaInput.value : null;

                            if (!file) {
                                alert("Vui lòng chọn tệp để tải lên.");
                                return;
                            }

                            var maxSize = 30 * 1024 * 1024;
                            if (file.size > maxSize) {
                                alert("File quá lớn! Vui lòng chọn file dưới 30MB.");
                                return;
                            }

                            var formData = new FormData();
                            formData.append("file", file);
                            formData.append("jsonData", JSON.stringify({ DotDanhGiaId, MinhChungId, DanhGia: danhGia }));

                            fetch(`/MinhChung/UploadMinhChungFile`, { method: "POST", body: formData })
                                .then(response => response.json())
                                .then(result => {
                                    alert(result.message);
                                    document.getElementById("eventSelect").dispatchEvent(new Event("change"));
                                })
                                .catch(error => alert(error));
                        });
                    });

                    document.querySelectorAll(".delete-btn").forEach(button => {
                        button.addEventListener("click", function (event) {
                            event.preventDefault();
                            var minhChungId = this.getAttribute("data-minhchung-id");
                            var fileLink = document.getElementById(`file-link-${minhChungId}`);
                            var filePath = fileLink ? fileLink.getAttribute("href") : null;

                            if (!filePath) {
                                alert("Lỗi: Đường dẫn tệp không hợp lệ.");
                                return;
                            }

                            if (!confirm("Bạn có chắc chắn muốn xóa minh chứng này?")) return;

                            fetch(`/MinhChung/DeleteUploadedFile`, {
                                method: "POST",
                                headers: { "Content-Type": "application/json" },
                                body: JSON.stringify({ filePath })
                            })
                                .then(response => response.text())
                                .then(result => {
                                    alert(result);
                                    document.getElementById("eventSelect").dispatchEvent(new Event("change"));
                                });
                        });
                    });
                });
        });
});


document.getElementById("exportExcelBtn").addEventListener("click", function () {
    var selectedEventId = document.getElementById("eventSelect").value;
        if (!selectedEventId) {
            alert("Vui lòng chọn một đợt đánh giá trước khi xuất Excel.");
            return;
    }

    window.location.href = `/MinhChung/ExportToExcel?eventId=${selectedEventId}`;
});

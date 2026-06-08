(function () {
    'use strict';

    let cameraList = (window.civExistingCameras || []).slice();
    let availableCameras = window.civAvailableCameras || [];

    document.addEventListener('DOMContentLoaded', function () {
        renderCameraList();
        wireAddButton();
        wireFormSubmit();
        wireActivoSwitch();
        wireTriggerDevices();
        updateActivoAvailability();
    });

    function wireActivoSwitch() {
        const input = document.getElementById('civ-activo');
        const track = document.getElementById('civ-activo-track');
        track.addEventListener('click', function () {
            if (!input.checked && !hasTriggerDevice()) {
                showActivoHint(true);
                return;
            }
            input.checked = !input.checked;
            showActivoHint(false);
        });
    }

    function wireTriggerDevices() {
        const lector = document.getElementById('ConfigLectorTarjetasId');
        lector.addEventListener('change', onTriggerChange);
        
        const sensor = document.getElementById('ConfigSensorVehicularId');
        sensor.addEventListener('change', onTriggerChange);
    }

    function onTriggerChange() {
        const input = document.getElementById('civ-activo');
        if (!hasTriggerDevice() && input.checked) {
            input.checked = false;
        }
        showActivoHint(false);
        updateActivoAvailability();
    }

    function hasTriggerDevice() {
        const lector = document.getElementById('ConfigLectorTarjetasId');
        const sensor = document.getElementById('ConfigSensorVehicularId');
        return (lector.value !== '') || (sensor.value !== '');
    }

    function showActivoHint(show) {
        const hint = document.getElementById('civ-activo-hint');
        hint.style.display = show ? '' : 'none';
    }

    function updateActivoAvailability() {
        const track = document.getElementById('civ-activo-track');
        if (!hasTriggerDevice()) {
            track.classList.add('civ-switch__track--disabled');
        } else {
            track.classList.remove('civ-switch__track--disabled');
            showActivoHint(false);
        }
    }

    function wireAddButton() {
        const btn = document.getElementById('civ-add-camera-btn');
        btn.addEventListener('click', function () {
            openCameraModal();
        });
    }

    function wireFormSubmit() {
        const form = document.getElementById('civ-form');
        form.addEventListener('submit', function () {
            syncHiddenInputs();
        });
        $(form).on('ajaxSubmit ajaxOptions', function () {
            syncHiddenInputs();
        });
    }

    function renderCameraList() {
        const container = document.getElementById('civ-cameras-list');
        container.innerHTML = '';

        cameraList.forEach(function (cam, idx) {
            const item = document.createElement('div');
            item.className = 'civ-camera';
            item.setAttribute('data-idx', idx);
            item.innerHTML =
                '<div class="civ-camera__icon"><span class="fa fa-video-camera"></span></div>' +
                '<div class="civ-camera__info">' +
                '  <div class="civ-camera__name">' + escHtml(cam.Nombre) + '</div>' +
                '  <div class="civ-camera__ip">IP: ' + escHtml(cam.Ip || '\u2014') + '</div>' +
                '</div>' +
                '<button type="button" class="civ-camera__delete" title="Quitar" aria-label="Quitar"><span class="fa fa-trash"></span></button>';

            item.querySelector('.civ-camera__delete').addEventListener('click', function () { removeCamera(idx); });

            container.appendChild(item);
        });

        syncHiddenInputs();
    }

    function removeCamera(idx) {
        cameraList.splice(idx, 1);
        renderCameraList();
    }

    function syncHiddenInputs() {
        const container = document.getElementById('civ-cameras-hidden');
        container.innerHTML = '';
        cameraList.forEach(function (cam) {
            const idInput = document.createElement('input');
            idInput.type = 'hidden';
            idInput.name = 'camaraIds';
            idInput.value = cam.Id;
            container.appendChild(idInput);
        });
    }

    function openCameraModal() {
        const alreadySelected = cameraList.map(function (c) { return c.Id; });
        const options = availableCameras.filter(function (c) {
            return alreadySelected.indexOf(c.Id) === -1;
        });

        if (options.length === 0) {
            alert('No hay cámaras disponibles para agregar.');
            return;
        }

        const backdrop = document.createElement('div');
        backdrop.className = 'civ-modal-backdrop';
        backdrop.id = 'civ-modal-backdrop';

        const modal = document.createElement('div');
        modal.className = 'civ-modal';
        modal.setAttribute('role', 'dialog');
        modal.setAttribute('aria-modal', 'true');
        modal.setAttribute('aria-labelledby', 'civ-modal-title');

        const optionsHtml = options.map(function (cam) {
            return '<div class="civ-modal__option" data-id="' + cam.Id + '" data-name="' + escAttr(cam.Nombre) + '" data-ip="' + escAttr(cam.Ip || '') + '" tabindex="0" role="button">' +
                '<div class="civ-camera__icon"><span class="fa fa-video-camera"></span></div>' +
                '<div class="civ-camera__info">' +
                '<div class="civ-camera__name">' + escHtml(cam.Nombre) + '</div>' +
                '<div class="civ-camera__ip">IP: ' + escHtml(cam.Ip || '\u2014') + '</div>' +
                '</div></div>';
        }).join('');

        modal.innerHTML =
            '<div class="civ-modal__header">' +
            '<span id="civ-modal-title">Seleccionar C\u00e1mara ALPR</span>' +
            '<button type="button" class="civ-modal__close" aria-label="Cerrar">&#x2715;</button>' +
            '</div>' +
            '<div class="civ-modal__body">' + optionsHtml + '</div>';

        backdrop.appendChild(modal);
        document.body.appendChild(backdrop);

        backdrop.addEventListener('click', function (e) {
            if (e.target === backdrop) closeModal();
        });
        modal.querySelector('.civ-modal__close').addEventListener('click', closeModal);

        modal.querySelectorAll('.civ-modal__option').forEach(function (opt) {
            function select() {
                cameraList.push({
                    Id: parseInt(opt.getAttribute('data-id'), 10),
                    Nombre: opt.getAttribute('data-name'),
                    Ip: opt.getAttribute('data-ip')
                });
                renderCameraList();
                closeModal();
            }
            opt.addEventListener('click', select);
            opt.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); select(); }
            });
        });

        modal.querySelector('.civ-modal__close').focus();
    }

    function closeModal() {
        const backdrop = document.getElementById('civ-modal-backdrop');
        backdrop.parentNode.removeChild(backdrop);
    }

    function escHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function escAttr(str) {
        return String(str).replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    }
})();
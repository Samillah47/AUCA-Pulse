/* AUCA Pulse — generic client-side table pagination.
 *
 * Usage:
 *   <table data-paginate="5"> ... </table>
 *
 * Effects:
 *   - Shows only `pageSize` tbody rows at a time.
 *   - Renders Previous / page-numbers / Next controls directly below the table.
 *   - Skips rows that have a `data-no-paginate` attribute (empty-state rows).
 *   - If the table has <=pageSize rows, the control is hidden.
 *
 * Search / filter integration: if an input is wired to hide rows with
 * style="display:none", call window.TablePaginate.refresh(table) afterwards
 * to repaginate the visible rows.
 */
(function () {
    function createEl(tag, attrs, text) {
        const el = document.createElement(tag);
        if (attrs) {
            for (const k in attrs) {
                if (k === 'class') el.className = attrs[k];
                else if (k === 'dataset') Object.assign(el.dataset, attrs[k]);
                else el.setAttribute(k, attrs[k]);
            }
        }
        if (text !== undefined) el.textContent = text;
        return el;
    }

    function eligibleRows(tbody) {
        return Array.from(tbody.rows).filter(r =>
            !r.hasAttribute('data-no-paginate') &&
            !r.hasAttribute('data-filtered-out')
        );
    }

    function hideFilteredOutRows(tbody) {
        Array.from(tbody.rows).forEach(r => {
            if (r.hasAttribute('data-filtered-out')) r.style.display = 'none';
        });
    }

    function render(table) {
        const tbody = table.tBodies[0];
        if (!tbody) return;

        const pageSize = Math.max(1, parseInt(table.getAttribute('data-paginate'), 10) || 5);
        const rows = eligibleRows(tbody);
        const total = rows.length;
        const pageCount = Math.max(1, Math.ceil(total / pageSize));

        // Find or build the pager container, placed right after the table
        let pager = table._pager;
        if (!pager) {
            pager = createEl('nav', { class: 'table-pager', 'aria-label': 'Table pagination' });
            // Place after the table (if table is inside .table-responsive wrapper, place after that wrapper)
            const host = table.closest('.table-responsive') || table;
            host.parentNode.insertBefore(pager, host.nextSibling);
            table._pager = pager;
        }

        hideFilteredOutRows(tbody);

        // Short-circuit when the table doesn't need a pager
        if (total <= pageSize) {
            rows.forEach(r => { r.style.display = ''; });
            pager.innerHTML = '';
            pager.style.display = 'none';
            return;
        }

        let currentPage = table._pagerPage || 1;
        if (currentPage > pageCount) currentPage = pageCount;
        if (currentPage < 1) currentPage = 1;
        table._pagerPage = currentPage;

        // Show only rows on the current page
        const start = (currentPage - 1) * pageSize;
        const end = start + pageSize;
        rows.forEach((row, i) => {
            row.style.display = (i >= start && i < end) ? '' : 'none';
        });

        // Build controls
        pager.innerHTML = '';
        pager.style.display = '';

        const group = createEl('ul', { class: 'pagination pagination-sm mb-0 mt-3 justify-content-end' });

        function addItem(label, page, opts) {
            opts = opts || {};
            const li = createEl('li', { class: 'page-item' + (opts.disabled ? ' disabled' : '') + (opts.active ? ' active' : '') });
            const btn = createEl('button', {
                class: 'page-link',
                type: 'button',
                'aria-label': opts.ariaLabel || label
            }, label);
            if (!opts.disabled && !opts.active) {
                btn.addEventListener('click', () => {
                    table._pagerPage = page;
                    render(table);
                });
            } else {
                btn.tabIndex = -1;
            }
            li.appendChild(btn);
            group.appendChild(li);
        }

        addItem('‹', currentPage - 1, { disabled: currentPage === 1, ariaLabel: 'Previous page' });

        // Compact page numbers: show all when small, else window around current
        const windowSize = 5;
        let first = 1, last = pageCount;
        if (pageCount > windowSize + 2) {
            first = Math.max(2, currentPage - 2);
            last = Math.min(pageCount - 1, currentPage + 2);
            addItem('1', 1, { active: currentPage === 1 });
            if (first > 2) addItem('…', 0, { disabled: true });
            for (let p = first; p <= last; p++) addItem(String(p), p, { active: currentPage === p });
            if (last < pageCount - 1) addItem('…', 0, { disabled: true });
            addItem(String(pageCount), pageCount, { active: currentPage === pageCount });
        } else {
            for (let p = 1; p <= pageCount; p++) addItem(String(p), p, { active: currentPage === p });
        }

        addItem('›', currentPage + 1, { disabled: currentPage === pageCount, ariaLabel: 'Next page' });

        const meta = createEl('div', { class: 'table-pager-meta text-muted small' },
            `Showing ${start + 1}-${Math.min(end, total)} of ${total}`);

        const wrap = createEl('div', { class: 'd-flex justify-content-between align-items-center flex-wrap gap-2' });
        wrap.appendChild(meta);
        wrap.appendChild(group);
        pager.appendChild(wrap);
    }

    function init() {
        document.querySelectorAll('table[data-paginate]').forEach(render);
    }

    // Expose for pages that filter rows dynamically
    window.TablePaginate = {
        refresh: render,
        init: init
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();

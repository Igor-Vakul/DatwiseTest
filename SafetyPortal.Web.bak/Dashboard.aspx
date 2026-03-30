<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs"
         Inherits="SafetyPortal.Web.Dashboard" MasterPageFile="~/Site.Master" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">Dashboard</asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle"    runat="server">
    <i class="bi bi-speedometer2 me-2 text-primary"></i>Dashboard
</asp:Content>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.3/dist/chart.umd.min.js"></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <%-- ── KPI Cards ──────────────────────────────────────────────────── --%>
    <div class="row g-3 mb-4">
        <div class="col-sm-6 col-xl-2">
            <div class="kpi-card bg-white">
                <div class="kpi-icon bg-primary-subtle text-primary">
                    <i class="bi bi-clipboard-data"></i>
                </div>
                <div>
                    <div class="kpi-label">Total Incidents</div>
                    <div class="kpi-value text-dark"><%= Stats.TotalIncidents %></div>
                </div>
            </div>
        </div>
        <div class="col-sm-6 col-xl-2">
            <div class="kpi-card bg-white">
                <div class="kpi-icon bg-warning-subtle text-warning">
                    <i class="bi bi-exclamation-circle"></i>
                </div>
                <div>
                    <div class="kpi-label">Open</div>
                    <div class="kpi-value text-warning"><%= Stats.OpenIncidents %></div>
                </div>
            </div>
        </div>
        <div class="col-sm-6 col-xl-2">
            <div class="kpi-card bg-white">
                <div class="kpi-icon bg-success-subtle text-success">
                    <i class="bi bi-check-circle"></i>
                </div>
                <div>
                    <div class="kpi-label">Closed</div>
                    <div class="kpi-value text-success"><%= Stats.ClosedIncidents %></div>
                </div>
            </div>
        </div>
        <div class="col-sm-6 col-xl-2">
            <div class="kpi-card bg-white">
                <div class="kpi-icon bg-danger-subtle text-danger">
                    <i class="bi bi-fire"></i>
                </div>
                <div>
                    <div class="kpi-label">High / Critical</div>
                    <div class="kpi-value text-danger"><%= Stats.HighCriticalIncidents %></div>
                </div>
            </div>
        </div>
        <div class="col-sm-6 col-xl-2">
            <div class="kpi-card bg-white">
                <div class="kpi-icon bg-secondary-subtle text-secondary">
                    <i class="bi bi-hourglass-split"></i>
                </div>
                <div>
                    <div class="kpi-label">Pending Actions</div>
                    <div class="kpi-value text-secondary"><%= Stats.PendingActions %></div>
                </div>
            </div>
        </div>
        <div class="col-sm-6 col-xl-2">
            <div class="kpi-card bg-white">
                <div class="kpi-icon bg-danger-subtle text-danger">
                    <i class="bi bi-alarm"></i>
                </div>
                <div>
                    <div class="kpi-label">Overdue Actions</div>
                    <div class="kpi-value text-danger"><%= Stats.OverdueActions %></div>
                </div>
            </div>
        </div>
    </div>

    <%-- ── Charts row ─────────────────────────────────────────────────── --%>
    <div class="row g-3 mb-4">
        <div class="col-lg-5">
            <div class="card sp-card h-100">
                <div class="card-header">
                    <i class="bi bi-pie-chart text-primary"></i> Incidents by Category
                </div>
                <div class="card-body d-flex align-items-center justify-content-center" style="height:260px">
                    <canvas id="chartCategory"></canvas>
                </div>
            </div>
        </div>
        <div class="col-lg-7">
            <div class="card sp-card h-100">
                <div class="card-header">
                    <i class="bi bi-bar-chart text-primary"></i> Incidents by Department
                </div>
                <div class="card-body" style="height:260px">
                    <canvas id="chartDept"></canvas>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-4">
        <div class="col-12">
            <div class="card sp-card">
                <div class="card-header">
                    <i class="bi bi-graph-up text-primary"></i> Incidents Trend (last 6 months)
                </div>
                <div class="card-body" style="height:220px">
                    <canvas id="chartTrend"></canvas>
                </div>
            </div>
        </div>
    </div>

    <%-- ── Recent Incidents ───────────────────────────────────────────── --%>
    <div class="card sp-card">
        <div class="card-header d-flex justify-content-between align-items-center">
            <span><i class="bi bi-clock-history text-primary"></i> Recent Incidents</span>
            <a href="<%= ResolveUrl("~/Incidents/List.aspx") %>" class="btn btn-sm btn-outline-primary">
                View All
            </a>
        </div>
        <div class="card-body p-0">
            <table class="table table-hover sp-table mb-0">
                <thead>
                    <tr>
                        <th>Report #</th>
                        <th>Title</th>
                        <th>Severity</th>
                        <th>Status</th>
                        <th>Date</th>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (var inc in Stats.RecentIncidents) { %>
                    <tr>
                        <td><a href="<%= ResolveUrl("~/Incidents/Details.aspx?id=" + inc.Id) %>"><code><%= inc.ReportNumber %></code></a></td>
                        <td><%= System.Web.HttpUtility.HtmlEncode(inc.Title) %></td>
                        <td><span class="badge badge-severity-<%= inc.SeverityLevel.ToLower() %>"><%= inc.SeverityLevel %></span></td>
                        <td><span class="badge badge-status-<%= inc.Status.ToLower() %>"><%= inc.Status %></span></td>
                        <td><%= inc.IncidentDate.ToString("dd MMM yyyy") %></td>
                    </tr>
                    <% } %>
                    <% if (Stats.RecentIncidents.Count == 0) { %>
                    <tr><td colspan="5" class="text-center text-muted py-3">No incidents recorded yet.</td></tr>
                    <% } %>
                </tbody>
            </table>
        </div>
    </div>

</asp:Content>

<asp:Content ContentPlaceHolderID="ScriptsContent" runat="server">
<script>
    const categoryData = <%=CategoryJson%>;
    const deptData     = <%=DeptJson%>;
    const trendData    = <%=TrendJson%>;

    const palette = ['#0d6efd','#6610f2','#fd7e14','#198754','#dc3545','#0dcaf0','#6c757d','#ffc107'];

    // Pie – by category
    new Chart(document.getElementById('chartCategory'), {
        type: 'doughnut',
        data: {
            labels: categoryData.map(x => x.label),
            datasets: [{ data: categoryData.map(x => x.count), backgroundColor: palette, borderWidth: 2 }]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: { legend: { position: 'right', labels: { boxWidth: 12 } } }
        }
    });

    // Bar – by department
    new Chart(document.getElementById('chartDept'), {
        type: 'bar',
        data: {
            labels: deptData.map(x => x.label),
            datasets: [{ label: 'Incidents', data: deptData.map(x => x.count),
                         backgroundColor: '#0d6efd99', borderColor: '#0d6efd', borderWidth: 1 }]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } }
        }
    });

    // Line – trend
    new Chart(document.getElementById('chartTrend'), {
        type: 'line',
        data: {
            labels: trendData.map(x => x.month),
            datasets: [{ label: 'Incidents', data: trendData.map(x => x.count),
                         fill: true, backgroundColor: '#0d6efd22', borderColor: '#0d6efd',
                         tension: .35, pointRadius: 4 }]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } }
        }
    });
</script>
</asp:Content>

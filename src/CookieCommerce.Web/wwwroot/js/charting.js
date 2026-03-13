
window.setupRevenueChart = (labels, values) => {
    const canvas = document.getElementById('revenueChart');
    if (!canvas) return;

    // Cleanup existing chart instance to prevent the "Identifier already declared" 
    // and "Canvas already in use" errors.
    let existingChart = Chart.getChart("revenueChart");
    if (existingChart) {
        existingChart.destroy();
    }

    const ctx = canvas.getContext('2d');
    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, 'rgba(234, 150, 84, 0.4)');
    gradient.addColorStop(1, 'rgba(234, 150, 84, 0)');

    const revenueChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Revenue ($)',
                data: values, // <-- This should be defined in your Razor view or passed from the server
                backgroundColor: gradient,
                borderColor: '#EA9654',
                borderWidth: 3,
                pointBackgroundColor: '#FFFFFF',
                pointBorderColor: '#EA9654',
                pointBorderWidth: 3,
                pointRadius: 6,
                pointHoverRadius: 8,
                tension: 0.4, // Curvy lines
                fill: true
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: '#2D2622',
                    titleColor: '#F4D58D',
                    bodyColor: '#FFFFFF',
                    padding: 12,
                    cornerRadius: 8,
                    displayColors: false,
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)',
                        drawBorder: false
                    },
                    ticks: {
                        font: {
                            family: "'Nunito', sans-serif"
                        },
                        color: '#9CA3AF'
                    }
                },
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        font: {
                            family: "'Nunito', sans-serif"
                        },
                        color: '#9CA3AF'
                    }
                }
            }
        }
    });
};
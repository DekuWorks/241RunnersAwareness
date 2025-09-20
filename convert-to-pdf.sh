#!/bin/bash

# 241 Runners Awareness - PDF Conversion Script
# This script helps convert the strategic roadmap to PDF

echo "🎯 241 Runners Awareness - Strategic Roadmap PDF Conversion"
echo "=========================================================="
echo ""

# Check if HTML file exists
if [ ! -f "STRATEGIC_ROADMAP_2025.html" ]; then
    echo "❌ Error: STRATEGIC_ROADMAP_2025.html not found!"
    echo "Please run: pandoc STRATEGIC_ROADMAP_2025.md -o STRATEGIC_ROADMAP_2025.html --standalone --css=styles.css"
    exit 1
fi

echo "✅ HTML file found: STRATEGIC_ROADMAP_2025.html"
echo ""

# Open the HTML file in the default browser
echo "🌐 Opening HTML file in your default browser..."
echo "   Once opened, you can:"
echo "   1. Press Cmd+P (Mac) or Ctrl+P (Windows/Linux)"
echo "   2. Select 'Save as PDF'"
echo "   3. Choose your desired settings"
echo "   4. Click 'Save'"
echo ""

# Try to open with different browsers/commands
if command -v open &> /dev/null; then
    echo "📱 Opening with macOS 'open' command..."
    open STRATEGIC_ROADMAP_2025.html
elif command -v xdg-open &> /dev/null; then
    echo "🐧 Opening with Linux 'xdg-open' command..."
    xdg-open STRATEGIC_ROADMAP_2025.html
elif command -v start &> /dev/null; then
    echo "🪟 Opening with Windows 'start' command..."
    start STRATEGIC_ROADMAP_2025.html
else
    echo "⚠️  Could not automatically open the file."
    echo "   Please manually open: STRATEGIC_ROADMAP_2025.html"
fi

echo ""
echo "📄 Alternative: You can also:"
echo "   1. Open the HTML file in any browser"
echo "   2. Use the browser's 'Print to PDF' feature"
echo "   3. Save as 'STRATEGIC_ROADMAP_2025.pdf'"
echo ""
echo "🎯 The roadmap includes:"
echo "   • Executive Summary"
echo "   • Current Achievements"
echo "   • 3-Phase Strategic Plan"
echo "   • Implementation Timeline"
echo "   • Success Metrics & KPIs"
echo "   • Resource Requirements"
echo "   • Risk Assessment"
echo "   • Next Immediate Actions"
echo ""
echo "✅ Ready to convert to PDF!"

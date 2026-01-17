### The Modernise the invoice template

> **Role:** Act as a Senior Frontend Developer and UI/UX Designer specializing in modern FinTech Dashboards.
> **Task:** Modernize an existing HTML invoice template. The goal is to achieve a "Tailwind/React Dashboard" aesthetic (clean, airy, professional) using **pure, standard CSS** (embedded in a `<style>` tag) with no external libraries or frameworks.
> **Design Requirements:**
> * **Typography:** Use a clean, modern sans-serif stack (e.g., Inter, system-ui). Use font-weight (400, 600, 700) to create a clear visual hierarchy.
> * **Color Palette:** Use a "Soft UI" approach. Neutral grays for secondary text (#6B7280), a deep slate for primary text (#111827), and one "Brand Accent" color (e.g., Indigo #4F46E5) for the logo or totals.
> * **Spacing & Layout:** Use generous padding and a 4px/8px grid logic. Use `display: flex` and `display: grid` for the layout instead of old-fashioned table hacks where possible.
> * **Tables:** The itemized list should have a clean, borderless look. Use a subtle bottom border (#F3F4F6) for rows and a light gray background for the header row with rounded corners.
> * **Visual Elements:** Include a "Status Badge" (e.g., a "Paid" or "Pending" pill with light background and darker text) and a clear, high-contrast "Total Due" section.
> 
> 
> **Technical Constraints:**
> * Single-file HTML/CSS solution.
> * Must be print-friendly (use `@media print` to ensure colors and spacing translate to PDF).
> * No JavaScript.
> 
> 
> **Input:** [Paste your current HTML code here]

---

### Why this prompt works

* **The "Tailwind" feel without the library:** By specifying "4px/8px grid logic" and "Neutral grays," you are telling the AI to mimic Tailwind’s default spacing and color scale (e.g., Gray 500, Gray 900).
* **Hierarchy focus:** Modern design isn't about more colors; it's about making the most important information (The Total and The Due Date) the easiest to find through font weight and size.
* **Print-friendliness:** Standard HTML invoices often break when saved as a PDF. Mentioning `@media print` ensures the AI handles background colors and margins correctly for the printer.

### Key elements to look for in the result

When the AI generates the code, check for these "modern" CSS properties to ensure it followed the instructions:

* **`border-radius: 8px`** or **`12px`**: This gives the "container" feel of a modern React component.
* **`border-collapse: collapse`**: Essential for making tables look clean.
* **`letter-spacing: -0.025em`**: A common trick used in modern dashboards to make large headings look more "premium."

**Would you like me to generate a base template using these exact principles so you can see a starting point?**
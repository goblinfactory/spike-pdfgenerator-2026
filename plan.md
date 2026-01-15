# plan

I need to test out a spike for a dockerised .net 8 C# web api that uses playwright chrome headless to create a pdf using a json file and a template.html. 
call the api POST create/pdf/templates/invoice
json {
 ... invoice data as json
}
hard code a templates folder 
/templates
   /invoice.html
   /delivery-note.html

constraints
1. MUST use a .net 8 compatible base image that can host both the pdfGenerator and the .net 8 webapi in the same container, NOT 2 different containers
2. This if for learning so MUST be as LEAN as possible, absolutely MVP
3. I actually need to focus on how to install chrome headless playwrite <-- this is the #1 priority in the learning I get out of this

Plan this out
ask me any questions  along the way to make sure we hit the learning goals
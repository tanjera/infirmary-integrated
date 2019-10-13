<?php

   include '../infirmary-server-access.php';

   $conn = new mysqli($dbhost, $dbuser, $dbpass, $dbname);

   if ($conn->connect_error) {
      die("Error: " . $conn->connect_error);
   }

   $sql = "SELECT version FROM versioning ORDER BY accession DESC LIMIT 1";

   $result = $conn->query($sql);

   if ($result->num_rows > 0) {
       while($row = $result->fetch_assoc()) {
           echo $row["version"] . "<<";
       }
   } else {
       echo "0.0<<";
   }

   $conn->close();
?>

<html>
   <body>


   </body>
</html>
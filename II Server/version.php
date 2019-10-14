<?php

   include '../infirmary-server-access.php';

   $conn = new mysqli($dbhost, $dbuser, $dbpass, $dbname);

   if ($conn->connect_error) {
      die();
   }

   try {
        $sql = "SELECT version FROM versioning ORDER BY accession DESC LIMIT 1";

        $result = $conn->query($sql);

        if ($result->num_rows > 0) {
            while($row = $result->fetch_assoc()) {
                echo $row["version"];
            }
        }
   } catch (Exception $e) {
       die();
   }
   
   $conn->close();
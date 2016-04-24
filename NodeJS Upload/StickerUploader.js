// ======== Sticker Uploader for Captain Cillian Application ========
// Import required resources
var express = require('express')
var router = express.Router();
var bodyParser = require('body-parser');
var multer  = require('multer')
var upload = multer()
var AWS = require('aws-sdk');
AWS.config.region = 'eu-west-1';

// =========== Initialize HTTP Server =============
var app = express();
// app.use(bodyParser.urlencoded({ extended: true }));

// Listen on specified port
var portNo = 8000;
var server = app.listen(portNo);

// =========== Initialize DynamoDB Client =============

var docClient = new AWS.DynamoDB.DocumentClient();
var tableName = "CaptainCillianStickers";
var themeSet = new Set();
var s3 = new AWS.S3();

var params = {
    TableName: tableName,
    ProjectionExpression: "Theme"
};

// =========== Initialize S3 Data ============
var accessKeyId =  process.env.AWS_ACCESS_KEY || "xxxxxx";
var secretAccessKey = process.env.AWS_SECRET_KEY || "+xxxxxx+B+xxxxxxx";

AWS.config.update({
    accessKeyId: accessKeyId,
    secretAccessKey: secretAccessKey
});

// =========== Scan DynamoDB Table =============

docClient.scan(params, onScan);

function onScan(err, data) {
    if (err) {
        console.error("Unable to scan the table. Error JSON:", JSON.stringify(err, null, 2));
    } else {
        // print all the themes
        data.Items.forEach(function(sticker) {
          themeSet.add(sticker.Theme);
        });

        // continue scanning if more stickers exist
        if (typeof data.LastEvaluatedKey != "undefined") {
            console.log("Scanning for more...");
            params.ExclusiveStartKey = data.LastEvaluatedKey;
            docClient.scan(params, onScan);
        }
    }
}

// ================================================
// ================= HTTP Server ==================

// Default message when user connects
app.get('/', function (req, res) {
  res.render('StickerUploader.ejs', { themes:themeSet });
});

// Sticker object constructor.
// Each new sticker to be uploaded will create a sticker object
var Sticker = function(stickerName, theme){
  this.stickerName = (stickerName) ? stickerName : "None";
  this.theme = (theme) ? theme : "None";
}

// Uses information from StickerUploader.ejs to select a file
// and either select a theme or create a new one.
// This is then uploaded to S3 as the image and DynamoDB as the image Bucket link
app.post('/upload/:theme', upload.single('sticker'), function(req, res) {

  var key = req.params.theme+'/'+req.file.originalname;

  console.log("KEY: " + key);
  var params = { Bucket: 'captain-cillian-bucket', Key: key, Body: req.file.buffer };

  s3.upload(params, function(err) {
    if(err) {
      console.log(err);
    }
  });

  // Parameters to upload image info to DynamoDB
  var params = {
    TableName: tableName,
    Item:{
      "Theme": req.params.theme,
      "StickerName": req.file.originalname
    }
  };

  docClient.put(params, function(err, data) {
    if (err) console.log(err);
    else console.log(data);
  });
});

// ===================== Fin ======================

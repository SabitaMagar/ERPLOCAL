-- Time Frame
CREATE TABLE PL_TIME_FRAME
(
  TIME_FRAME_CODE     NUMBER(4)                 NOT NULL,
  TIME_FRAME_EDESC    VARCHAR2(100 BYTE)        NOT NULL,
  TIME_FRAME_NDESC    VARCHAR2(400 BYTE)        DEFAULT NULL,
  TIME_FRAME_TYPE     VARCHAR2(20 BYTE)         DEFAULT 'CALENDAR',
  DAYS                NUMBER(3)                 DEFAULT NULL,
  REMARKS             VARCHAR2(400 BYTE)        DEFAULT NULL,
  COMPANY_CODE        VARCHAR2(30 BYTE),
  CREATED_BY          VARCHAR2(30 BYTE),
  CREATED_DATE        DATE,
  LAST_MODIFIED_BY    VARCHAR2(30 BYTE),
  LAST_MODIFIED_DATE  DATE,
  APPROVED_BY         VARCHAR2(30 BYTE),
  APPROVED_DATE       DATE,
  DELETED_FLAG        CHAR(1 BYTE)
)

CREATE UNIQUE INDEX PK_TIME_FRAME ON PL_TIME_FRAME
(TIME_FRAME_CODE)
LOGGING
TABLESPACE USERS
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MAXSIZE          UNLIMITED
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
            FLASH_CACHE      DEFAULT
            CELL_FLASH_CACHE DEFAULT
           )
NOPARALLEL;


CREATE UNIQUE INDEX UNQ_TIME_FRAME ON PL_TIME_FRAME
(TIME_FRAME_EDESC, COMPANY_CODE)
LOGGING
TABLESPACE USERS
PCTFREE    10
INITRANS   2
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MAXSIZE          UNLIMITED
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
            FLASH_CACHE      DEFAULT
            CELL_FLASH_CACHE DEFAULT
           )
NOPARALLEL;


ALTER TABLE PL_TIME_FRAME ADD (
  CHECK (TIME_FRAME_TYPE IN ('CALENDAR', 'DAYS'))
  ENABLE VALIDATE,
  CONSTRAINT PK_TIME_FRAME
  PRIMARY KEY
  (TIME_FRAME_CODE)
  USING INDEX PK_TIME_FRAME
  ENABLE VALIDATE,
  CONSTRAINT UNQ_TIME_FRAME
  UNIQUE (TIME_FRAME_EDESC, COMPANY_CODE)
  USING INDEX UNQ_TIME_FRAME
  ENABLE VALIDATE);
 --End Time Frame
 
 -- Sales Plan
 CREATE TABLE PL_SALES_PLAN
(
  PLAN_CODE           NUMBER(12)                NOT NULL,
  PLAN_EDESC          VARCHAR2(200 BYTE)        NOT NULL,
  PLAN_NDESC          VARCHAR2(400 BYTE)        DEFAULT NULL,
  SALES_QUANTITY      NUMBER(16,2)              DEFAULT NULL,
  SALES_AMOUNT        NUMBER(16,2)              DEFAULT NULL,
  TIME_FRAME_CODE     NUMBER(4)                 NOT NULL,
  CALENDAR_TYPE       VARCHAR2(3 BYTE),
  START_DATE          DATE                      NOT NULL,
  END_DATE            DATE                      NOT NULL,
  SALES_PRICE_TYPE    VARCHAR2(50 BYTE),
  REMARKS             VARCHAR2(400 BYTE)        DEFAULT NULL,
  COMPANY_CODE        VARCHAR2(30 BYTE),
  BRANCH_CODE         VARCHAR2(30 BYTE),
  CREATED_BY          VARCHAR2(30 BYTE),
  CREATED_DATE        DATE,
  LAST_MODIFIED_BY    VARCHAR2(30 BYTE),
  LAST_MODIFIED_DATE  DATE,
  APPROVED_FLAG       CHAR(1 BYTE),
  APPROVED_BY         VARCHAR2(30 BYTE),
  APPROVED_DATE       DATE,
  DELETED_FLAG        CHAR(1 BYTE)
)
CREATE UNIQUE INDEX UNQ_SALES_PLAN ON PL_SALES_PLAN
(PLAN_EDESC, TIME_FRAME_CODE, START_DATE, END_DATE)
NOPARALLEL;


CREATE UNIQUE INDEX PK_SALES_PLAN ON PL_SALES_PLAN
(PLAN_CODE)
NOPARALLEL;


ALTER TABLE PL_SALES_PLAN ADD (
  CHECK ( CALENDAR_TYPE IN ('ENG','LOC'))
  ENABLE VALIDATE,
  CONSTRAINT PK_SALES_PLAN
  PRIMARY KEY
  (PLAN_CODE)
  USING INDEX PK_SALES_PLAN
  ENABLE VALIDATE,
  CONSTRAINT UNQ_SALES_PLAN
  UNIQUE (PLAN_EDESC, TIME_FRAME_CODE, START_DATE, END_DATE)
  USING INDEX UNQ_SALES_PLAN
  ENABLE VALIDATE);
--End sales plan

-- sales plan detail
CREATE TABLE PL_SALES_PLAN_DTL
(
  PLAN_CODE            NUMBER(12)               NOT NULL,
  PLAN_DATE            DATE                     NOT NULL,
  PER_DAY_QUANTITY     NUMBER(14,2)             DEFAULT NULL,
  PER_DAY_AMOUNT       NUMBER(14,2)             DEFAULT NULL,
  ITEM_CODE            VARCHAR2(30 BYTE)        DEFAULT NULL,
  CUSTOMER_CODE        VARCHAR2(30 BYTE)        DEFAULT NULL,
  EMPLOYEE_CODE        VARCHAR2(30 BYTE)        DEFAULT NULL,
  DIVISION_CODE        VARCHAR2(30 BYTE)        DEFAULT NULL,
  FREQUENCY_JSON       VARCHAR2(500 BYTE)       DEFAULT NULL,
  SALES_PRICE_APPLIED  NUMBER(16,2)             DEFAULT NULL,
  REMARKS              VARCHAR2(400 BYTE)       DEFAULT NULL,
  COMPANY_CODE         VARCHAR2(30 BYTE),
  BRANCH_CODE          VARCHAR2(30 BYTE),
  CREATED_BY           VARCHAR2(30 BYTE),
  CREATED_DATE         DATE,
  LAST_MODIFIED_BY     VARCHAR2(30 BYTE),
  LAST_MODIFIED_DATE   DATE,
  APPROVED_FLAG        CHAR(1 BYTE),
  APPROVED_BY          VARCHAR2(30 BYTE),
  APPROVED_DATE        DATE,
  DELETED_FLAG         CHAR(1 BYTE)
)

CREATE UNIQUE INDEX UNQ_SALES_PLAN_DTL ON PL_SALES_PLAN_DTL
(PLAN_CODE, PLAN_DATE, ITEM_CODE, CUSTOMER_CODE, DIVISION_CODE, 
EMPLOYEE_CODE)
NOPARALLEL;


ALTER TABLE PL_SALES_PLAN_DTL ADD (
  CONSTRAINT UNQ_SALES_PLAN_DTL
  UNIQUE (PLAN_CODE, PLAN_DATE, ITEM_CODE, CUSTOMER_CODE, DIVISION_CODE, EMPLOYEE_CODE)
  USING INDEX UNQ_SALES_PLAN_DTL
  ENABLE VALIDATE);

ALTER TABLE PL_SALES_PLAN_DTL ADD (
  CONSTRAINT FK_SALES_PLAN_DTL 
  FOREIGN KEY (PLAN_CODE) 
  REFERENCES PL_SALES_PLAN (PLAN_CODE)
  ENABLE VALIDATE);  
-- end sales plan detail  

-- master sales plan
CREATE TABLE PL_MASTER_SALES_PLAN
(
  MASTER_PLAN_CODE    NUMBER(12)                NOT NULL,
  MASTER_PLAN_EDESC   VARCHAR2(500 BYTE)        NOT NULL,
  MASTER_PLAN_NDESC   VARCHAR2(500 BYTE)        NOT NULL,
  START_DATE          DATE                      NOT NULL,
  END_DATE            DATE                      NOT NULL,
  CUSTOMER_CODE       VARCHAR2(30 BYTE)         DEFAULT NULL,
  EMPLOYEE_CODE       VARCHAR2(30 BYTE)         DEFAULT NULL,
  DIVISION_CODE       VARCHAR2(30 BYTE)         DEFAULT NULL,
  REMARKS             VARCHAR2(400 BYTE)        DEFAULT NULL,
  COMPANY_CODE        VARCHAR2(30 BYTE),
  BRANCH_CODE         VARCHAR2(30 BYTE),
  CREATED_BY          VARCHAR2(30 BYTE),
  CREATED_DATE        DATE,
  LAST_MODIFIED_BY    VARCHAR2(30 BYTE),
  LAST_MODIFIED_DATE  DATE,
  APPROVED_FLAG       CHAR(1 BYTE),
  APPROVED_BY         VARCHAR2(30 BYTE),
  APPROVED_DATE       DATE,
  DELETED_FLAG        CHAR(1 BYTE)
)
CREATE UNIQUE INDEX PK_MASTER_SALES_PLAN ON PL_MASTER_SALES_PLAN
(MASTER_PLAN_CODE)
NOPARALLEL;


ALTER TABLE PL_MASTER_SALES_PLAN ADD (
  CONSTRAINT PK_MASTER_SALES_PLAN
  PRIMARY KEY
  (MASTER_PLAN_CODE)
  USING INDEX PK_MASTER_SALES_PLAN
  ENABLE VALIDATE);
-- end master sales plan

-- sales plan map
CREATE TABLE PL_SALES_PLAN_MAP
(
  MASTER_PLAN_CODE  NUMBER(12)                  NOT NULL,
  PLAN_CODE         NUMBER(12)                  NOT NULL,
  PARENT_PLAN_CODE  NUMBER(12)                  DEFAULT NULL,
  PLAN_FLAG         CHAR(2 BYTE)                DEFAULT 'SP'
)  

ALTER TABLE PL_SALES_PLAN_MAP ADD (
  CONSTRAINT FK_SALES_PLAN_MAP1 
  FOREIGN KEY (MASTER_PLAN_CODE) 
  REFERENCES PL_MASTER_SALES_PLAN (MASTER_PLAN_CODE)
  ENABLE VALIDATE,
  CONSTRAINT FK_SALES_PLAN_MAP2 
  FOREIGN KEY (MASTER_PLAN_CODE) 
  REFERENCES PL_MASTER_SALES_PLAN (MASTER_PLAN_CODE)
  ENABLE VALIDATE);
-- end of sales plan map  

-- hr employee tree
CREATE TABLE HR_EMPLOYEE_TREE
(
  EMPLOYEE_CODE         VARCHAR2(30 BYTE)       NOT NULL,
  PARENT_EMPLOYEE_CODE  VARCHAR2(30 BYTE)
)

ALTER TABLE HR_EMPLOYEE_TREE ADD COMPANY_CODE  varchar2(30); 
--OR
DROP TABLE HR_EMPLOYEE_TREE CASCADE CONSTRAINTS;

CREATE TABLE HR_EMPLOYEE_TREE
(
  EMPLOYEE_CODE         VARCHAR2(30 BYTE)       NOT NULL,
  PARENT_EMPLOYEE_CODE  VARCHAR2(30 BYTE),
  COMPANY_CODE          VARCHAR2(30 BYTE)
)
-- end hr employee tree

-- sunday april 23, 2017
CREATE TABLE  PL_PREFERENCE_SETUP
(
  COMPANY_CODE         VARCHAR2(30 BYTE),
  BRANCH_CODE          VARCHAR2(30 BYTE),
  ITEM_GROUP_ENTRY       CHAR(1 BYTE) DEFAULT 'N',
  CONSTRAINT PK_PL_PREF_SETUP PRIMARY KEY (COMPANY_CODE,BRANCH_CODE)
);

INSERT INTO PL_PREFERENCE_SETUP(COMPANY_CODE,BRANCH_CODE,ITEM_GROUP_ENTRY) VALUES('01','01.01','Y');

-- This query about creating table PL_COA_PLAN &  PL_COA_PLAN_DTL is from script generated by TOAD,
-- as query was written and implemented in DB by Pramod sir, he could brif about the detail.
CREATE TABLE PL_COA_PLAN
(
  PLAN_CODE           NUMBER(12)                NOT NULL,
  PLAN_EDESC          VARCHAR2(200 BYTE)        NOT NULL,
  PLAN_NDESC          VARCHAR2(400 BYTE)        DEFAULT NULL,
  SALES_AMOUNT        NUMBER(16,2)              DEFAULT NULL,
  TIME_FRAME_CODE     NUMBER(4)                 NOT NULL,
  CALENDAR_TYPE       VARCHAR2(3 BYTE)          DEFAULT 'LOC',
  START_DATE          DATE                      NOT NULL,
  END_DATE            DATE                      NOT NULL,
  REMARKS             VARCHAR2(400 BYTE)        DEFAULT NULL,
  COMPANY_CODE        VARCHAR2(30 BYTE),
  BRANCH_CODE         VARCHAR2(30 BYTE),
  CREATED_BY          VARCHAR2(30 BYTE),
  CREATED_DATE        DATE,
  LAST_MODIFIED_BY    VARCHAR2(30 BYTE),
  LAST_MODIFIED_DATE  DATE,
  APPROVED_FLAG       CHAR(1 BYTE)              DEFAULT 'N',
  APPROVED_BY         VARCHAR2(30 BYTE),
  APPROVED_DATE       DATE,
  DELETED_FLAG        CHAR(1 BYTE)              DEFAULT 'N'
)
CREATE UNIQUE INDEX PK_COA_PLAN ON PL_COA_PLAN
(PLAN_CODE)
NOPARALLEL;

CREATE UNIQUE INDEX UNQ_COA_PLAN ON PL_COA_PLAN
(PLAN_EDESC, START_DATE, END_DATE)
NOPARALLEL;

ALTER TABLE PL_COA_PLAN ADD (
  CONSTRAINT CHK_COA_APPROVED_FLAG
  CHECK (APPROVED_FLAG IN ('Y', 'N')),
  CONSTRAINT CHK_COA_CALENDAR_TYPE
  CHECK (CALENDAR_TYPE IN ('ENG', 'LOC')),
  CONSTRAINT CHK_COA_DELETED_FLAG
  CHECK (DELETED_FLAG IN ('Y', 'N')),
  CONSTRAINT PK_COA_PLAN
  PRIMARY KEY
  (PLAN_CODE)
  USING INDEX PK_COA_PLAN,
  CONSTRAINT UNQ_COA_PLAN
  UNIQUE (PLAN_EDESC, START_DATE, END_DATE)
  USING INDEX UNQ_COA_PLAN);

-- table pl_coa_plan_dtl
CREATE TABLE PL_COA_PLAN_DTL
(
  PLAN_CODE           NUMBER(12)                NOT NULL,
  PLAN_DATE           DATE                      NOT NULL,
  PER_DAY_AMOUNT      NUMBER(14,2)              DEFAULT NULL,
  ACC_CODE            VARCHAR2(30 BYTE)         DEFAULT NULL,
  DIVISION_CODE       VARCHAR2(30 BYTE)         DEFAULT NULL,
  FREQUENCY_JSON      VARCHAR2(500 BYTE)        DEFAULT NULL,
  REMARKS             VARCHAR2(400 BYTE)        DEFAULT NULL,
  COMPANY_CODE        VARCHAR2(30 BYTE),
  BRANCH_CODE         VARCHAR2(30 BYTE),
  CREATED_BY          VARCHAR2(30 BYTE),
  CREATED_DATE        DATE,
  LAST_MODIFIED_BY    VARCHAR2(30 BYTE),
  LAST_MODIFIED_DATE  DATE,
  APPROVED_FLAG       CHAR(1 BYTE)              DEFAULT 'N',
  APPROVED_BY         VARCHAR2(30 BYTE),
  APPROVED_DATE       DATE,
  DELETED_FLAG        CHAR(1 BYTE)              DEFAULT 'N'
)

CREATE UNIQUE INDEX UNQ_COA_PLAN_DTL ON PL_COA_PLAN_DTL
(PLAN_CODE, PLAN_DATE, ACC_CODE, DIVISION_CODE, BRANCH_CODE, 
COMPANY_CODE)
NOPARALLEL;

ALTER TABLE PL_COA_PLAN_DTL ADD (
  CONSTRAINT CHK_COA_DTL_APP_FLAG
  CHECK (APPROVED_FLAG IN ('Y', 'N')),
  CONSTRAINT CHK_COA_DTL_DEL_FLAG
  CHECK (DELETED_FLAG IN ('Y', 'N')),
  CONSTRAINT UNQ_COA_PLAN_DTL
  UNIQUE (PLAN_CODE, PLAN_DATE, ACC_CODE, DIVISION_CODE, BRANCH_CODE, COMPANY_CODE)
  USING INDEX UNQ_COA_PLAN_DTL);

ALTER TABLE PL_COA_PLAN_DTL ADD (
  CONSTRAINT FK_COA_PLAN_DTL 
  FOREIGN KEY (PLAN_CODE) 
  REFERENCES PL_COA_PLAN (PLAN_CODE));


  alter TABLE  PL_PREFERENCE_SETUP 
add SHOW_CUSTOMER VARCHAR(1 BYTE) DEFAULT 'N';


ALTER TABLE PL_PREFERENCE_SETUP
ADD SHOW_BRANCH VARCHAR(1 BYTE) DEFAULT 'N';


ALTER TABLE PL_PREFERENCE_SETUP
ADD SHOW_EMPLOYEE VARCHAR(1 BYTE) DEFAULT 'N';



SET DEFINE OFF;

Insert into PL_TIME_FRAME
   (TIME_FRAME_CODE, TIME_FRAME_EDESC, TIME_FRAME_TYPE, COMPANY_CODE, CREATED_BY, CREATED_DATE, LAST_MODIFIED_BY, LAST_MODIFIED_DATE, APPROVED_BY, APPROVED_DATE, DELETED_FLAG)
 Values
   (202, 'WEEK', 'CALENDAR', 
    '06', '2', TO_DATE('07/15/2017 13:32:16', 'MM/DD/YYYY HH24:MI:SS'), '2', 
    TO_DATE('07/15/2017 13:32:16', 'MM/DD/YYYY HH24:MI:SS'), '2', TO_DATE('07/15/2017 13:32:16', 'MM/DD/YYYY HH24:MI:SS'), 'N');
    
Insert into PL_TIME_FRAME
   (TIME_FRAME_CODE, TIME_FRAME_EDESC, TIME_FRAME_TYPE, COMPANY_CODE, CREATED_BY, CREATED_DATE, LAST_MODIFIED_BY, LAST_MODIFIED_DATE, APPROVED_BY, APPROVED_DATE, DELETED_FLAG)
 Values
   (204, 'DAILY', 'CALENDAR', 
    '06', '2', TO_DATE('07/15/2017 13:32:16', 'MM/DD/YYYY HH24:MI:SS'), '2', 
    TO_DATE('07/15/2017 13:32:16', 'MM/DD/YYYY HH24:MI:SS'), '2', TO_DATE('07/15/2017 13:32:16', 'MM/DD/YYYY HH24:MI:SS'), 'N');
    
Insert into PL_TIME_FRAME
   (TIME_FRAME_CODE, TIME_FRAME_EDESC, TIME_FRAME_TYPE, COMPANY_CODE, CREATED_BY, CREATED_DATE, LAST_MODIFIED_BY, LAST_MODIFIED_DATE, APPROVED_BY, APPROVED_DATE, DELETED_FLAG)
 Values
   (203, 'MONTH', 'CALENDAR', 
    '06', '2', TO_DATE('07/15/2017 13:32:16', 'MM/DD/YYYY HH24:MI:SS'), '2', 
    TO_DATE('07/15/2017 13:32:16', 'MM/DD/YYYY HH24:MI:SS'), '2', TO_DATE('07/15/2017 13:32:16', 'MM/DD/YYYY HH24:MI:SS'), 'N');
    
    
COMMIT;
